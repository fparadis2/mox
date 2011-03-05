using System;
using System.Collections.Generic;

using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.Lobby.Backend
{
    [TestFixture]
    public class LobbyBackendTests
    {
        #region Variables

        private MockRepository m_mockery;

        private LogContext m_logContext;
        private LobbyServiceBackend m_lobbyService;
        private LobbyBackend m_lobby;

        private MockClient m_client1;
        private MockClient m_client2;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();

            m_logContext = new LogContext();
            m_lobbyService = new LobbyServiceBackend(m_logContext);
            m_lobby = new LobbyBackend(m_lobbyService);

            m_client1 = CreateClient(new User("John"));
            m_client2 = CreateClient(new User("Jack"));
        }

        #endregion

        #region Utilities

        private MockClient CreateClient(User user)
        {
            return new MockClient(m_mockery, user);
        }

        private static IDisposable Expect_OnUserChanged(MockClient client, UserChange change, params User[] users)
        {
            EventSink<UserChangedEventArgs> sink = new EventSink<UserChangedEventArgs>();
            client.UserChanged += sink;

            List<User> actualUsers = new List<User>();

            sink.Callback += (o, e) =>
            {
                Assert.AreEqual(change, e.Change);
                actualUsers.Add(e.User);
            };

            return new DisposableHelper(() =>
            {
                Assert.Collections.AreEquivalent(users, actualUsers);
                client.UserChanged -= sink;
            });
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Id_is_always_unique()
        {
            Assert.AreNotEqual(new LobbyBackend(m_lobbyService).Id, new LobbyBackend(m_lobbyService).Id);
        }

        [Test]
        public void Test_Construction_values()
        {
            Assert.Collections.IsEmpty(m_lobby.Users);
        }

        [Test]
        public void Test_Users_returns_the_list_of_logged_users()
        {
            m_lobby.Login(m_client1);
            m_lobby.Login(m_client2);

            Assert.Collections.AreEquivalent(new[] { m_client1.User, m_client2.User }, m_lobby.Users);

            m_lobby.Logout(m_client1);

            Assert.Collections.AreEquivalent(new[] { m_client2.User }, m_lobby.Users);
        }

        [Test]
        public void Test_Login_does_nothing_if_already_logged_in()
        {
            m_lobby.Login(m_client1);
            m_lobby.Login(m_client1);

            Assert.Collections.AreEquivalent(new[] { m_client1.User }, m_lobby.Users);
        }

        [Test]
        public void Test_Logout_does_nothing_if_user_is_not_logged_in()
        {
            m_lobby.Logout(m_client1);

            Assert.Collections.IsEmpty(m_lobby.Users);
        }

        [Test]
        public void Test_ChatService_works()
        {
            m_lobby.Login(m_client1);
            m_lobby.Login(m_client2);

            m_client2.Expect_Chat_Message(m_client1.User, "Hello");

            using (m_mockery.Test())
            {
                m_lobby.ChatService.Say(m_client1.User, "Hello");
            }
        }

        [Test]
        public void Test_Login_immediatly_sends_the_client_the_list_of_users_except_the_new_user()
        {
            using (Expect_OnUserChanged(m_client1, UserChange.Joined))
            {
                m_lobby.Login(m_client1);
            }

            using (Expect_OnUserChanged(m_client1, UserChange.Joined, m_client2.User))
            using (Expect_OnUserChanged(m_client2, UserChange.Joined, m_client1.User))
            {
                m_lobby.Login(m_client2);
            }

            var client3 = CreateClient(new User("Albert"));

            using (Expect_OnUserChanged(m_client1, UserChange.Joined, client3.User))
            using (Expect_OnUserChanged(m_client2, UserChange.Joined, client3.User))
            using (Expect_OnUserChanged(client3, UserChange.Joined, m_client1.User, m_client2.User))
            {
                m_lobby.Login(client3);
            }
        }

        [Test]
        public void Test_Logout_sends_the_change_to_all_other_users()
        {
            m_lobby.Login(m_client1);
            m_lobby.Login(m_client2);

            using (Expect_OnUserChanged(m_client1, UserChange.Left, m_client2.User))
            {
                m_lobby.Logout(m_client2);
            }
        }

        #endregion
    }
}
