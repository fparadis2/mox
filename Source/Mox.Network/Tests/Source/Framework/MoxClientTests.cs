// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Net.Sockets;
using System.ServiceModel;
using System.Diagnostics;
using System.ServiceModel.Channels;

using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using Is = Rhino.Mocks.Constraints.Is;

namespace Mox.Network
{
    [TestFixture]
    public class MoxClientTests
    {
        #region Inner Types

        public interface IMoxClientImplementation
        {
            IMoxService CreateProxyMoxService(IMoxClient implementation, Binding binding, EndpointAddress address);
            IChatPrivateService CreateProxyChatService(IChatClient implementation, Binding binding, EndpointAddress address);

            string GetSessionId(object service);
        }

        private class MockMoxClient : MoxClient
        {
            #region Variables

            private IMoxClientImplementation m_implementation;

            #endregion

            #region Constructor

            public MockMoxClient(IMoxClientImplementation implementation, IMoxClient client, IChatClient chatClient)
                : base(client, chatClient)
            {
                Debug.Assert(implementation != null);
                m_implementation = implementation;
            }

            #endregion

            #region Methods

            protected override IMoxService CreateProxyMoxService(IMoxClient implementation, Binding binding, EndpointAddress address)
            {
                return m_implementation.CreateProxyMoxService(implementation, binding, address);
            }

            protected override IChatPrivateService CreateProxyChatService(IChatClient implementation, Binding binding, EndpointAddress address)
            {
                return m_implementation.CreateProxyChatService(implementation, binding, address);
            }

            protected override string GetSessionId<TService>(TService service)
            {
                return m_implementation.GetSessionId(service);
            }

            #endregion
        }

        #endregion

        #region Variables

        private MockRepository m_mockery;

        private IMoxClient m_moxClient;
        private IChatClient m_chatClient;

        private MoxClient m_client;
        private IMoxClientImplementation m_implementation;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();

            m_moxClient = m_mockery.StrictMock<IMoxClient>();
            m_chatClient = m_mockery.StrictMock<IChatClient>();

            m_implementation = m_mockery.StrictMock<IMoxClientImplementation>();
            m_client = new MockMoxClient(m_implementation, m_moxClient, m_chatClient);

            m_client.Host = "MyHost";
            m_client.Port = 54;
        }

        #endregion

        #region Utilities

        private IMoxService Expect_CreateMoxProxyService(IMoxClient clientImplementation, EndpointAddress address)
        {
            IMoxService moxService = m_mockery.StrictMultiMock<IMoxService>(typeof(ICommunicationObject));
            Expect.Call(m_implementation.CreateProxyMoxService(null, null, null)).Return(moxService);
            ApplyCreateConstraints(clientImplementation, address);

            return moxService;
        }

        private IChatPrivateService Expect_CreateChatProxyService(IChatClient clientImplementation, EndpointAddress address)
        {
            IChatPrivateService moxService = m_mockery.StrictMultiMock<IChatPrivateService>(typeof(ICommunicationObject));
            Expect.Call(m_implementation.CreateProxyChatService(null, null, null)).Return(moxService);
            ApplyCreateConstraints(clientImplementation, address);
            return moxService;
        }

        private void ApplyCreateConstraints(object clientImplementation, EndpointAddress address)
        {
            LastCall.Constraints(
                Is.Equal(clientImplementation),
                Is.NotNull(),
                Is.Equal(address));
        }

        private void Expect_OpenCommunicationObject(object communicationObject)
        {
            Expect_OpenCommunicationObject(communicationObject, CommunicationState.Created);
        }

        private void Expect_OpenCommunicationObject(object communicationObject, CommunicationState state)
        {
            ICommunicationObject comObject = (ICommunicationObject)communicationObject;
            Expect.Call(comObject.State).Return(state);

            if (state == CommunicationState.Created)
            {
                comObject.Open();
                comObject.Closed += delegate { }; LastCall.IgnoreArguments();
                comObject.Faulted += delegate { }; LastCall.IgnoreArguments();
            }
        }

        private void Expect_CloseCommunicationObject(object communicationObject)
        {
            Expect_CloseCommunicationObject(communicationObject, CommunicationState.Opened);
        }

        private void Expect_CloseCommunicationObject(object communicationObject, CommunicationState state)
        {
            Expect_CloseCommunicationObject(communicationObject, state, false);
        }

        private void Expect_CloseCommunicationObject(object communicationObject, CommunicationState state, bool throwExceptionOnClose)
        {
            ICommunicationObject comObject = (ICommunicationObject)communicationObject;
            Expect.Call(comObject.State).Return(state);

            comObject.Closed -= delegate { }; LastCall.IgnoreArguments();
            comObject.Faulted -= delegate { }; LastCall.IgnoreArguments();

            if (state == CommunicationState.Opened)
            {
                comObject.Close();

                if (throwExceptionOnClose)
                {
                    LastCall.Throw(new Exception());
                }
            }            
        }

        private IMethodOptions<string> Expect_GetSessionId(object service)
        {
            return Expect.Call(m_implementation.GetSessionId(service));
        }

        private void Mock_Connect()
        {
            IMoxService moxService = Expect_CreateMoxProxyService(m_moxClient, new EndpointAddress(ServiceUtilities.GetServiceAddress(ServiceUtilities.Constants.MoxServiceName, "MyHost", 54)));
            IChatPrivateService chatService = Expect_CreateChatProxyService(m_chatClient, new EndpointAddress(ServiceUtilities.GetServiceAddress(ServiceUtilities.Constants.ChatServiceName, "MyHost", 54)));

            Expect_OpenCommunicationObject(moxService);
            Expect_OpenCommunicationObject(chatService);

            Expect.Call(moxService.Login("Georges")).Return(new LoginDetails(LoginResult.AlreadyLoggedIn, new Client("Georges")));

            m_mockery.Test(delegate
            {
                Assert.IsTrue(m_client.Connect());
            });
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new MoxClient(null, m_chatClient); });
            Assert.Throws<ArgumentNullException>(delegate { new MoxClient(m_moxClient, null); });
        }

        [Test]
        public void Test_Can_get_set_the_host()
        {
            m_client.Host = "MyHost";
            Assert.AreEqual("MyHost", m_client.Host);
        }

        [Test]
        public void Test_Can_get_set_the_port()
        {
            m_client.Port = 10;
            Assert.AreEqual(10, m_client.Port);
        }

        [Test]
        public void Test_Connect_logins_on_all_the_services()
        {
            IMoxService moxService = Expect_CreateMoxProxyService(m_moxClient, new EndpointAddress(ServiceUtilities.GetServiceAddress(ServiceUtilities.Constants.MoxServiceName, "MyHost", 54)));
            IChatPrivateService chatService = Expect_CreateChatProxyService(m_chatClient, new EndpointAddress(ServiceUtilities.GetServiceAddress(ServiceUtilities.Constants.ChatServiceName, "MyHost", 54)));

            Expect_OpenCommunicationObject(moxService);
            Expect_OpenCommunicationObject(chatService);

            Expect.Call(moxService.Login("Georges")).Return(new LoginDetails(LoginResult.Success, new Client("Georges")));
            Expect_GetSessionId(moxService).Return("SessionId");
            Expect.Call(chatService.Login("SessionId")).Return(true);

            m_mockery.Test(delegate
            {
                Assert.IsTrue(m_client.Connect());
                Assert.IsTrue(m_client.IsConnected);
            });
        }

        [Test]
        public void Test_Connecting_when_already_connected_does_nothing()
        {
            IMoxService moxService = Expect_CreateMoxProxyService(m_moxClient, new EndpointAddress(ServiceUtilities.GetServiceAddress(ServiceUtilities.Constants.MoxServiceName, "MyHost", 54)));
            IChatPrivateService chatService = Expect_CreateChatProxyService(m_chatClient, new EndpointAddress(ServiceUtilities.GetServiceAddress(ServiceUtilities.Constants.ChatServiceName, "MyHost", 54)));

            Expect_OpenCommunicationObject(moxService);
            Expect_OpenCommunicationObject(chatService);

            Expect.Call(moxService.Login("Georges")).Return(new LoginDetails(LoginResult.AlreadyLoggedIn, new Client("Georges")));

            m_mockery.Test(delegate
            {
                Assert.IsTrue(m_client.Connect());
                Assert.IsTrue(m_client.IsConnected);
            });
        }

        [Test]
        public void Test_Connecting_twice_does_nothing()
        {
            IMoxService moxService = Expect_CreateMoxProxyService(m_moxClient, new EndpointAddress(ServiceUtilities.GetServiceAddress(ServiceUtilities.Constants.MoxServiceName, "MyHost", 54)));
            IChatPrivateService chatService = Expect_CreateChatProxyService(m_chatClient, new EndpointAddress(ServiceUtilities.GetServiceAddress(ServiceUtilities.Constants.ChatServiceName, "MyHost", 54)));

            Expect_OpenCommunicationObject(moxService);
            Expect_OpenCommunicationObject(chatService);

            Expect.Call(moxService.Login("Georges")).Return(new LoginDetails(LoginResult.AlreadyLoggedIn, new Client("Georges")));

            m_mockery.Test(delegate
            {
                Assert.IsTrue(m_client.Connect());
                Assert.IsTrue(m_client.Connect());
            });
        }

        [Test]
        public void Test_Disconnecting_closes_all_communication_objects()
        {
            IMoxService moxService = Expect_CreateMoxProxyService(m_moxClient, new EndpointAddress(ServiceUtilities.GetServiceAddress(ServiceUtilities.Constants.MoxServiceName, "MyHost", 54)));
            IChatPrivateService chatService = Expect_CreateChatProxyService(m_chatClient, new EndpointAddress(ServiceUtilities.GetServiceAddress(ServiceUtilities.Constants.ChatServiceName, "MyHost", 54)));

            Expect_OpenCommunicationObject(moxService);
            Expect_OpenCommunicationObject(chatService);

            Expect.Call(moxService.Login("Georges")).Return(new LoginDetails(LoginResult.AlreadyLoggedIn, new Client("Georges")));

            Expect_CloseCommunicationObject(moxService);
            Expect_CloseCommunicationObject(chatService);

            m_mockery.Test(delegate
            {
                Assert.IsTrue(m_client.Connect());
                m_client.Disconnect();
                Assert.IsFalse(m_client.IsConnected);
            });
        }

        [Test]
        public void Test_Disconnecting_only_closes_opened_communication_objects()
        {
            IMoxService moxService = Expect_CreateMoxProxyService(m_moxClient, new EndpointAddress(ServiceUtilities.GetServiceAddress(ServiceUtilities.Constants.MoxServiceName, "MyHost", 54)));
            IChatPrivateService chatService = Expect_CreateChatProxyService(m_chatClient, new EndpointAddress(ServiceUtilities.GetServiceAddress(ServiceUtilities.Constants.ChatServiceName, "MyHost", 54)));

            Expect_OpenCommunicationObject(moxService);
            Expect_OpenCommunicationObject(chatService);

            Expect.Call(moxService.Login("Georges")).Return(new LoginDetails(LoginResult.AlreadyLoggedIn, new Client("Georges")));

            Expect_CloseCommunicationObject(moxService, CommunicationState.Faulted);
            Expect_CloseCommunicationObject(chatService);

            m_mockery.Test(delegate
            {
                Assert.IsTrue(m_client.Connect());
                m_client.Disconnect();
                Assert.IsFalse(m_client.IsConnected);
            });
        }

        [Test]
        public void Test_Disconnecting_is_exception_safe()
        {
            IMoxService moxService = Expect_CreateMoxProxyService(m_moxClient, new EndpointAddress(ServiceUtilities.GetServiceAddress(ServiceUtilities.Constants.MoxServiceName, "MyHost", 54)));
            IChatPrivateService chatService = Expect_CreateChatProxyService(m_chatClient, new EndpointAddress(ServiceUtilities.GetServiceAddress(ServiceUtilities.Constants.ChatServiceName, "MyHost", 54)));

            Expect_OpenCommunicationObject(moxService);
            Expect_OpenCommunicationObject(chatService);

            Expect.Call(moxService.Login("Georges")).Return(new LoginDetails(LoginResult.AlreadyLoggedIn, new Client("Georges")));

            Expect_CloseCommunicationObject(moxService, CommunicationState.Opened, true);
            Expect_CloseCommunicationObject(chatService); // Chat service still gets closed correctly.

            m_mockery.Test(delegate
            {
                Assert.IsTrue(m_client.Connect());
                m_client.Disconnect();
                Assert.IsFalse(m_client.IsConnected);
            });
        }

        [Test]
        public void Test_Disconnecting_when_not_connected_does_nothing()
        {
            m_mockery.Test(delegate
            {
                m_client.Disconnect();
            });
        }

        [Test]
        public void Test_Cannot_change_parameters_while_connected()
        {
            Mock_Connect();

            Assert.Throws<InvalidOperationException>(delegate { m_client.Host = "NewHost"; });
            Assert.Throws<InvalidOperationException>(delegate { m_client.Port = 15; });
        }

        [Test]
        public void Test_Can_access_the_chat_service_if_connected()
        {
            Mock_Connect();
            Assert.IsNotNull(m_client.ChatService);
        }

        #endregion
    }
}
