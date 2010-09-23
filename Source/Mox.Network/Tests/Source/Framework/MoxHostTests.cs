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

using NUnit.Framework;
using Rhino.Mocks;

using Is = Rhino.Mocks.Constraints.Is;

namespace Mox.Network
{
    [TestFixture]
    public class MoxHostTests
    {
        #region Inner Types

        public interface IMoxHostImplementation
        {
            MoxHost.IServiceHost CreateServiceHost(object singletonInstance);
        }

        private class MockMoxHost : MoxHost
        {
            private readonly IMoxHostImplementation m_implementation;

            public MockMoxHost(IMoxHostImplementation implementation, IOperationContext operationContext, IMainService mainService)
                : base(operationContext, mainService)
            {
                Throw.IfNull(implementation, "implementation");
                m_implementation = implementation;
            }

            protected override IServiceHost CreateServiceHost(object singletonInstance)
            {
                return m_implementation.CreateServiceHost(singletonInstance);
            }
        }

        #endregion

        #region Variables

        private MockRepository m_mockery;

        private IMoxHostImplementation m_implementation;
        private IOperationContext m_context;
        private IMainService m_mainService;

        private MoxHost m_host;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();

            m_implementation = m_mockery.StrictMock<IMoxHostImplementation>();
            m_context = m_mockery.StrictMock<IOperationContext>();
            m_mainService = m_mockery.StrictMock<IMainService>();

            m_host = new MockMoxHost(m_implementation, m_context, m_mainService);
        }

        #endregion

        #region Utilities

        private MoxHost.IServiceHost Expect_CreateHost(object singletonInstance, Type type, string address)
        {
            var host = m_mockery.StrictMock<MoxHost.IServiceHost>();
            Expect.Call(m_implementation.CreateServiceHost(singletonInstance)).Return(host);

            host.AddServiceEndpoint(null, null, null); LastCall.Constraints(Is.Equal(type), Is.NotNull(), Is.Equal(address));
            host.Faulted += null; LastCall.IgnoreArguments();
            host.Open();

            return host;
        }

        private void Expect_CreateHost(object singletonInstance)
        {
            var host = m_mockery.StrictMock<MoxHost.IServiceHost>();
            Expect.Call(m_implementation.CreateServiceHost(singletonInstance)).Return(host);

            host.AddServiceEndpoint(null, null, null); LastCall.IgnoreArguments();
            host.Faulted += null; LastCall.IgnoreArguments();
            host.Open();
        }

        private void Host_Open()
        {
            Expect.Call(m_context.LocalHostDns).Return("my.local.host");

            Expect_CreateHost(m_mainService);
            Expect_CreateHost(m_host.ChatService);

            m_mockery.Test(delegate
            {
                Assert.IsTrue(m_host.Open());
            });
        }

        #endregion

        #region Tests

        [Test]
        public void Test_construction_values()
        {
            Assert.AreEqual(m_context, m_host.OperationContext);
            Assert.AreEqual(m_mainService, m_host.MainService);
        }

        [Test]
        public void Test_Host_contains_a_chat_service()
        {
            Assert.IsNotNull(m_host.ChatService);
        }

        [Test]
        public void Test_Can_get_set_the_port()
        {
            m_host.Port = 90;
            Assert.AreEqual(90, m_host.Port);
        }

        #region Open / Close

        [Test]
        public void Test_Open_returns_false_if_cannot_get_the_Dns_for_the_local_host()
        {
            Expect.Call(m_context.LocalHostDns).Throw(new SocketException());

            m_mockery.Test(delegate
            {
                Assert.IsFalse(m_host.Open());
            });
        }

        [Test]
        public void Test_Open_opens_all_services()
        {
            m_host.Port = 1;

            Expect.Call(m_context.LocalHostDns).Return("my.local.host");

            Expect_CreateHost(m_mainService, typeof(IMoxService), "net.tcp://my.local.host:1/MoxService");
            Expect_CreateHost(m_host.ChatService, typeof(IChatPrivateService), "net.tcp://my.local.host:1/ChatService");

            m_mockery.Test(delegate
            {
                Assert.IsTrue(m_host.Open());
            });
        }

        [Test]
        public void Test_Cannot_change_the_port_when_the_host_is_open()
        {
            Host_Open();

            Assert.Throws<InvalidOperationException>(delegate { m_host.Port = 10; });
        }

        [Test]
        public void Test_IsOpen_returns_true_when_the_host_has_been_opened()
        {
            Assert.IsFalse(m_host.IsOpen);
            Host_Open();
            Assert.IsTrue(m_host.IsOpen);
        }

        [Test]
        public void Test_Close_closes_all_hosts()
        {
            m_host.Port = 1;

            Expect.Call(m_context.LocalHostDns).Return("my.local.host");

            var mainHost = Expect_CreateHost(m_mainService, typeof(IMoxService), "net.tcp://my.local.host:1/MoxService");
            var chatHost = Expect_CreateHost(m_host.ChatService, typeof(IChatPrivateService), "net.tcp://my.local.host:1/ChatService");

            mainHost.Close(TimeSpan.FromSeconds(2));
            chatHost.Close(TimeSpan.FromSeconds(2));

            m_mockery.Test(delegate
            {
                Assert.IsTrue(m_host.Open());
                m_host.Close();
                Assert.IsFalse(m_host.IsOpen);
            });
        }

        [Test]
        public void Test_Hosts_that_cannot_be_closed_within_the_time_are_aborted()
        {
            m_host.Port = 1;

            Expect.Call(m_context.LocalHostDns).Return("my.local.host");

            var mainHost = Expect_CreateHost(m_mainService, typeof(IMoxService), "net.tcp://my.local.host:1/MoxService");
            var chatHost = Expect_CreateHost(m_host.ChatService, typeof(IChatPrivateService), "net.tcp://my.local.host:1/ChatService");

            mainHost.Close(TimeSpan.FromSeconds(2)); LastCall.Throw(new TimeoutException());
            mainHost.Abort();
            chatHost.Close(TimeSpan.FromSeconds(2));

            m_mockery.Test(delegate
            {
                Assert.IsTrue(m_host.Open());
                m_host.Close();
                Assert.IsFalse(m_host.IsOpen);
            });
        }

        [Test]
        public void Test_Closing_an_already_closed_host_does_nothing()
        {
            m_mockery.Test(delegate
            {
                m_host.Close();
            });
        }

        [Test]
        public void Test_Opening_an_already_opened_host_throws()
        {
            Host_Open();

            m_mockery.Test(delegate
            {
                Assert.Throws<InvalidOperationException>(delegate { m_host.Open(); });
            });
        }

        #endregion

        #region Logging

        [Test]
        public void Test_Can_add_loggers_to_the_Logs_collection()
        {
            Assert.IsNotNull(m_host.Logs);
            Assert.IsFalse(m_host.Logs.IsReadOnly);

            ILog log = m_mockery.StrictMock<ILog>();
            m_host.Logs.Add(log);
            Assert.Collections.Contains(log, m_host.Logs);
        }

        [Test]
        public void Test_Log_logs_to_each_registered_logger()
        {
            ILog log1 = m_mockery.StrictMock<ILog>();
            ILog log2 = m_mockery.StrictMock<ILog>();

            m_host.Logs.Add(log1);
            m_host.Logs.Add(log2);

            LogMessage message = new LogMessage() { Text = "MyMessage" };

            using (m_mockery.Unordered())
            {
                log1.Log(message);
                log2.Log(message);
            }

            m_mockery.Test(delegate
            {
                m_host.Log(message);
            });
        }

        #endregion

        #endregion
    }
}
