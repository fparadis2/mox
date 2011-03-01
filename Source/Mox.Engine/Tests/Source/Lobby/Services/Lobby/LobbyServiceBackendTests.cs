using System;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.Lobby.Backend
{
    [TestFixture]
    public class LobbyServiceBackendTests
    {
        #region Variables

        private MockRepository m_mockery;

        private LobbyServiceBackend m_lobbyService;

        private MockClient m_client1;
        private MockClient m_client2;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();
            m_lobbyService = new LobbyServiceBackend();

            m_client1 = CreateClient(new User("John"));
            m_client2 = CreateClient(new User("Jack"));
        }

        #endregion

        #region Utilities

        private MockClient CreateClient(User user)
        {
            return new MockClient(m_mockery, user);
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
            var lobby = m_lobbyService.CreateLobby(m_client1);

            Assert.IsNotNull(lobby);
            Assert.Collections.Contains(m_client1.User, lobby.Users);
            Assert.Collections.AreEquivalent(new[] { lobby }, m_lobbyService.Lobbies);
        }

        [Test]
        public void Test_JoinLobby_returns_null_if_the_lobby_doesnt_exist()
        {
            var lobby = m_lobbyService.JoinLobby(Guid.NewGuid(), m_client1);

            Assert.IsNull(lobby);
            Assert.Collections.IsEmpty(m_lobbyService.Lobbies);
        }

        [Test]
        public void Test_JoinLobby_returns_a_lobby_instance_for_an_existing_lobby()
        {
            var lobby1 = m_lobbyService.CreateLobby(m_client1);
            var lobby2 = m_lobbyService.JoinLobby(lobby1.Id, m_client2);

            Assert.AreSame(lobby1, lobby2);
            Assert.Collections.Contains(m_client2.User, lobby2.Users);
            Assert.Collections.AreEquivalent(new[] { lobby1 }, m_lobbyService.Lobbies);
        }

        [Test]
        public void Test_Lobby_closes_when_last_user_leaves()
        {
            var lobby = m_lobbyService.CreateLobby(m_client1);
            m_lobbyService.JoinLobby(lobby.Id, m_client2);

            lobby.Logout(m_client2);
            Assert.Collections.AreEquivalent(new[] { lobby }, m_lobbyService.Lobbies);

            lobby.Logout(m_client1);
            Assert.Collections.IsEmpty(m_lobbyService.Lobbies);
        }

        [Test]
        public void Test_GetLobby_returns_the_lobby_with_the_given_id()
        {
            var lobby = m_lobbyService.CreateLobby(m_client1);

            Assert.IsNotNull(lobby);
            Assert.AreEqual(lobby, m_lobbyService.GetLobby(lobby.Id));
            Assert.IsNull(m_lobbyService.GetLobby(Guid.NewGuid()));
        }

        #endregion
    }
}
