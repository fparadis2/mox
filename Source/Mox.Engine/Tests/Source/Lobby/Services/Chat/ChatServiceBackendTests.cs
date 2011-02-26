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

using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.Lobby
{
    [TestFixture]
    public class ChatServiceBackendTests
    {
        #region Variables

        private MockRepository m_mockery;

        private ChatServiceBackend m_service;

        private User m_normalUser;
        private IChatClient m_normalUserClient;

        private User m_spectatorUser;
        private IChatClient m_spectatorUserClient;

        private User m_otherUser;
        private IChatClient m_otherUserClient;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();

            m_service = new ChatServiceBackend();

            m_normalUser = new User("Joe");
            m_normalUserClient = m_mockery.StrictMock<IChatClient>();

            m_spectatorUser = new User("Spectator");
            m_spectatorUserClient = m_mockery.StrictMock<IChatClient>();

            m_otherUser = new User("Other");
            m_otherUserClient = m_mockery.StrictMock<IChatClient>();

            m_service.Register(m_normalUser, m_normalUserClient, ChatLevel.Normal);
            m_service.Register(m_spectatorUser, m_spectatorUserClient, ChatLevel.Spectator);
            m_service.Register(m_otherUser, m_otherUserClient, ChatLevel.Spectator);
        }

        #endregion

        #region Utilities

        private void Say(User user, string message)
        {
            m_mockery.Test(() => m_service.Say(user, message));
        }

        private static void Expect_Receive(IChatClient client, User user, string msg)
        {
            client.OnMessageReceived(null);
            LastCall.Callback<MessageReceivedEventArgs>(e =>
            {
                Assert.AreEqual(user, e.User);
                Assert.AreEqual(msg, e.Message);
                return true;
            });
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Register_fails_if_user_is_already_registered()
        {
            Assert.Throws<ArgumentException>(() => m_service.Register(m_normalUser, m_normalUserClient, ChatLevel.Normal));
        }

        [Test]
        public void Test_Unregister_removes_a_client()
        {
            m_service.Unregister(m_normalUser);

            using (m_mockery.Unordered())
            {
                Expect_Receive(m_otherUserClient, m_spectatorUser, "Hello");
            }

            Say(m_spectatorUser, "Hello");
        }

        [Test]
        public void Test_Say_broadcasts_the_message_to_registered_users()
        {
            using (m_mockery.Unordered())
            {
                Expect_Receive(m_spectatorUserClient, m_normalUser, "Hello");
                Expect_Receive(m_otherUserClient, m_normalUser, "Hello");
            }

            Say(m_normalUser, "Hello");
        }

        [Test]
        public void Test_Say_only_broadcasts_the_message_to_compatible_users()
        {
            using (m_mockery.Unordered())
            {
                Expect_Receive(m_otherUserClient, m_spectatorUser, "Hello");
            }

            Say(m_spectatorUser, "Hello");
        }

        [Test]
        public void Test_Unregistered_users_are_ignored()
        {
            m_service.Unregister(m_normalUser);

            Say(m_normalUser, "Hello");
        }

        #endregion
    }
}
