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
using System.ServiceModel;

using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.Network
{
    [TestFixture]
    public class ChatServiceTests : ServiceTestsHelper
    {
        #region Variables

        private ChatService m_service;
        private IChatClient m_client1;
        private IChatClient m_client2;

        #endregion

        #region Setup

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            m_service = new ChatService(m_serviceManager);

            m_client1 = m_mockery.StrictMultiMock<IChatClient>(typeof(ICommunicationObject));
            m_client2 = m_mockery.StrictMultiMock<IChatClient>(typeof(ICommunicationObject));
        }

        #endregion

        #region Utilities

        private string GetMainSessionId(string name)
        {
            return "Main " + name;
        }

        private Client Login(IChatClient chatClient, string name)
        {
            Client client = new Client(name);

            Expect.Call(m_mainService.GetClient(GetMainSessionId(name))).Return(client);

            Expect_GetCallbackChannel(chatClient);
            Expect.Call(m_operationContext.SessionId).Return(name);

            Expect_Log(LogImportance.Debug, string.Format("User {0} [{1}] logged in to chat service", name, GetMainSessionId(name)));

            m_mockery.Test(delegate
            {
                Assert.IsTrue(m_service.Login(GetMainSessionId(name)));
            });

            SetBasicExpectations();

            return client;
        }

        private void Logout(IChatClient chatClient, string name)
        {
            Expect_GetCallbackChannel(chatClient);
            Expect.Call(m_operationContext.SessionId).Return(name);

            Expect_Log(LogImportance.Debug, string.Format("User [{0}] logged out from chat service", GetMainSessionId(name)));

            m_mockery.Test(delegate
            {
                m_service.Logout();
            });

            SetBasicExpectations();
        }

        private void Expect_GetSpeaker(Client client)
        {
            Expect.Call(m_operationContext.SessionId).Return(client.Name);
            Expect.Call(m_mainService.GetClient(GetMainSessionId(client.Name))).Return(client);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new ChatService(null); });
        }

        [Test]
        public void Test_Login_with_an_invalid_session_id_returns_false()
        {
            Expect.Call(m_mainService.GetClient("Invalid")).Return(null);

            m_mockery.Test(() => Assert.IsFalse(m_service.Login("Invalid")));
        }

        [Test]
        public void Test_Login_with_an_valid_session_id_returns_true()
        {
            Expect.Call(m_mainService.GetClient("SessionId")).Return(new Client("DummyClient"));

            Expect_GetCallbackChannel(m_client1);
            Expect.Call(m_operationContext.SessionId).Return("Client1SessionId");

            Expect_Log(LogImportance.Debug, string.Format("User {0} [{1}] logged in to chat service", "DummyClient", "SessionId"));

            m_mockery.Test(() => Assert.IsTrue(m_service.Login("SessionId")));
        }

        [Test]
        public void Test_Say_will_send_the_text_to_all_logged_in_players()
        {
            Client client1 = Login(m_client1, "Client1");
            Client client2 = Login(m_client2, "Client2");

            using (m_mockery.Ordered())
            {
                // First message
                Expect_GetSpeaker(client1);

                Expect_Log(LogImportance.Low, string.Format("User {0} says: {1}", client1.Name, "Hello"));

                using (m_mockery.Unordered())
                {
                    m_client1.ClientTalked(client1, "Hello");
                    m_client2.ClientTalked(client1, "Hello");
                }

                // Second message
                Expect_GetSpeaker(client2);

                Expect_Log(LogImportance.Low, string.Format("User {0} says: {1}", client2.Name, "Hey!"));

                using (m_mockery.Unordered())
                {
                    m_client1.ClientTalked(client2, "Hey!");
                    m_client2.ClientTalked(client2, "Hey!");
                }
            }

            m_mockery.Test(delegate
            {
                m_service.Say("Hello");
                m_service.Say("Hey!");
            });
        }

        [Test]
        public void Test_Logged_out_clients_dont_receive_the_message()
        {
            Client client1 = Login(m_client1, "Client1");
            Client client2 = Login(m_client2, "Client2");

            Logout(m_client1, client1.Name);

            using (m_mockery.Ordered())
            {
                // First message
                Expect_GetSpeaker(client2);

                Expect_Log(LogImportance.Low, string.Format("User {0} says: {1}", client2.Name, "Hey!"));

                using (m_mockery.Unordered())
                {
                    m_client1.ClientTalked(client2, "Hey!"); LastCall.IgnoreArguments().Repeat.Never();
                    m_client2.ClientTalked(client2, "Hey!");
                }
            }

            m_mockery.Test(delegate
            {
                m_service.Say("Hey!");
            });
        }

        [Test]
        public void Test_Cannot_speak_with_a_non_logged_in_client()
        {
            Client client1 = Login(m_client1, "Client1");

            m_client1.ClientTalked(null, null); LastCall.IgnoreArguments().Repeat.Never();

            using (m_mockery.Ordered())
            {
                Expect.Call(m_operationContext.SessionId).Return(client1.Name);
                Expect.Call(m_mainService.GetClient(GetMainSessionId(client1.Name))).Return(null); // Invalid client

                Expect.Call(m_operationContext.SessionId).Return("Invalid"); // Invalid session id
            }

            m_mockery.Test(delegate
            {
                m_service.Say("Hey!");

                m_service.Say("Ho!");
            });
        }

        [Test]
        public void Test_Clients_are_dropped_if_something_goes_wrong()
        {
            Client client1 = Login(m_client1, "Client1");

            using (m_mockery.Ordered())
            {
                // First message
                Expect_GetSpeaker(client1);

                Expect_Log(LogImportance.Low, string.Format("User {0} says: {1}", client1.Name, "Hello"));

                using (m_mockery.Unordered())
                {
                    m_client1.ClientTalked(client1, "Hello");
                    LastCall.Throw(new Exception());

                    Expect.Call(((ICommunicationObject)m_client1).State).Return(CommunicationState.Opened);
                    ((ICommunicationObject)m_client1).Abort();
                }

                // Second message
                Expect_GetSpeaker(client1);

                Expect_Log(LogImportance.Low, string.Format("User {0} says:", client1.Name));
            }

            m_mockery.Test(delegate
            {
                m_service.Say("Hello");
                m_service.Say("This message does nothing because the first client was dropped out because he threw at us :(");
            });
        }

        #endregion
    }
}
