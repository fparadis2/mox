using System;
using NUnit.Framework;

namespace Mox.Lobby.Backend
{
    [TestFixture]
    public class LobbyBackendTests
    {
        #region Variables

        private LobbyServiceBackend m_lobbyService;
        private LobbyBackend m_lobby;

        private User m_user1;
        private User m_user2;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_lobbyService = new LobbyServiceBackend();
            m_lobby = new LobbyBackend(m_lobbyService);

            m_user1 = new User("John");
            m_user2 = new User("Jack");
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
            m_lobby.Login(m_user1);
            m_lobby.Login(m_user2);

            Assert.Collections.AreEquivalent(new[] { m_user1, m_user2 }, m_lobby.Users);

            m_lobby.Logout(m_user1);

            Assert.Collections.AreEquivalent(new[] { m_user2 }, m_lobby.Users);
        }

        [Test]
        public void Test_Login_does_nothing_if_already_logged_in()
        {
            m_lobby.Login(m_user1);
            m_lobby.Login(m_user1);

            Assert.Collections.AreEquivalent(new[] { m_user1 }, m_lobby.Users);
        }

        [Test]
        public void Test_Logout_does_nothing_if_user_is_not_logged_in()
        {
            m_lobby.Logout(m_user1);

            Assert.Collections.IsEmpty(m_lobby.Users);
        }

        #endregion
    }
}
