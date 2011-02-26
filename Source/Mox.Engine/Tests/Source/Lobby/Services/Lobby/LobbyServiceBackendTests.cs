using System;
using NUnit.Framework;

namespace Mox.Lobby.Backend
{
    [TestFixture]
    public class LobbyServiceBackendTests
    {
        #region Variables

        private LobbyServiceBackend m_lobbyService;

        private User m_user1;
        private User m_user2;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_lobbyService = new LobbyServiceBackend();

            m_user1 = new User("John");
            m_user2 = new User("Jack");
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.Collections.IsEmpty(m_lobbyService.Lobbies);
        }

        [Test]
        public void Test_CreateLobby_creates_a_new_lobby()
        {
            var lobby = m_lobbyService.CreateLobby(m_user1);

            Assert.IsNotNull(lobby);
            Assert.Collections.Contains(m_user1, lobby.Users);
            Assert.Collections.AreEquivalent(new[] { lobby }, m_lobbyService.Lobbies);
        }

        [Test]
        public void Test_JoinLobby_returns_null_if_the_lobby_doesnt_exist()
        {
            var lobby = m_lobbyService.JoinLobby(Guid.NewGuid(), m_user1);

            Assert.IsNull(lobby);
            Assert.Collections.IsEmpty(m_lobbyService.Lobbies);
        }

        [Test]
        public void Test_JoinLobby_returns_a_lobby_instance_for_an_existing_lobby()
        {
            var lobby1 = m_lobbyService.CreateLobby(m_user1);
            var lobby2 = m_lobbyService.JoinLobby(lobby1.Id, m_user2);

            Assert.AreSame(lobby1, lobby2);
            Assert.Collections.Contains(m_user2, lobby2.Users);
            Assert.Collections.AreEquivalent(new[] { lobby1 }, m_lobbyService.Lobbies);
        }

        [Test]
        public void Test_Lobby_closes_when_last_user_leaves()
        {
            var lobby = m_lobbyService.CreateLobby(m_user1);
            m_lobbyService.JoinLobby(lobby.Id, m_user2);

            lobby.Logout(m_user2);
            Assert.Collections.AreEquivalent(new[] { lobby }, m_lobbyService.Lobbies);

            lobby.Logout(m_user1);
            Assert.Collections.IsEmpty(m_lobbyService.Lobbies);
        }

        #endregion
    }
}
