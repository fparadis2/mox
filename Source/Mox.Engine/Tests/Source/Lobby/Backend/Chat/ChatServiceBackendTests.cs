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
using System.Linq;
using NUnit.Framework;

namespace Mox.Lobby.Backend
{
    [TestFixture]
    public class ChatServiceBackendTests
    {
        #region Variables

        private LogContext m_logContext;
        private ChatServiceBackend m_service;

        private MockClient m_normalUser;
        private MockClient m_spectatorUser;
        private MockClient m_otherUser;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_logContext = new LogContext();
            m_service = new ChatServiceBackend(m_logContext);

            m_normalUser = new MockClient("Joe");

            m_spectatorUser = new MockClient("Spectator");

            m_otherUser = new MockClient("Other");

            m_service.Register(m_normalUser.User, m_normalUser.Channel, ChatLevel.Normal);
            m_service.Register(m_spectatorUser.User, m_spectatorUser.Channel, ChatLevel.Spectator);
            m_service.Register(m_otherUser.User, m_otherUser.Channel, ChatLevel.Spectator);
        }

        #endregion

        #region Utilities

        private void Say(MockClient userClient, string message)
        {
            m_service.Say(userClient.Channel, message);
        }

        private static IDisposable Expect_Receive(MockClient client, MockClient speaker, string msg)
        {
            client.Channel.SentMessages.Clear();

            return new DisposableHelper(() =>
            {
                ChatMessage message = client.Channel.SentMessages.OfType<ChatMessage>().Single();

                Assert.AreEqual(msg, message.Message);
                Assert.AreEqual(speaker.User, message.User);
            });
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Register_fails_if_user_is_already_registered()
        {
            Assert.Throws<ArgumentException>(() => m_service.Register(m_normalUser.User, m_normalUser.Channel, ChatLevel.Normal));
        }

        [Test]
        public void Test_Unregister_removes_a_client()
        {
            m_service.Unregister(m_normalUser.Channel);

            using (Expect_Receive(m_otherUser, m_spectatorUser, "Hello"))
            {
                Say(m_spectatorUser, "Hello");
            }
        }

        [Test]
        public void Test_Say_broadcasts_the_message_to_registered_users()
        {
            using (Expect_Receive(m_otherUser, m_normalUser, "Hello"))
            using (Expect_Receive(m_spectatorUser, m_normalUser, "Hello"))
            {
                Say(m_normalUser, "Hello");
            }
        }

        [Test]
        public void Test_Say_only_broadcasts_the_message_to_compatible_users()
        {
            using (Expect_Receive(m_otherUser, m_spectatorUser, "Hello"))
            {
                Say(m_spectatorUser, "Hello");
            }
        }

        [Test]
        public void Test_Unregistered_users_are_ignored()
        {
            m_service.Unregister(m_normalUser.Channel);

            Say(m_normalUser, "Hello");
        }

        #endregion
    }
}
