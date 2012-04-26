using System;
using NUnit.Framework;

namespace Mox.Lobby2.Backend
{
    [TestFixture]
    public class LobbyServiceBackendTests
    {
        #region Variables

        private LogContext m_logContext;
        private LobbyServiceBackend m_lobbyService;

        private MockClient m_client1;
        private MockClient m_client2;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_logContext = new LogContext();
            m_lobbyService = new LobbyServiceBackend(m_logContext);

            m_client1 = new MockClient("John");
            m_client2 = new MockClient("Jack");
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
            var lobby = m_lobbyService.CreateLobby(m_client1.Channel, m_client1.User);

            Assert.IsNotNull(lobby);
            Assert.Collections.Contains(m_client1.User, lobby.Users);
            Assert.Collections.AreEquivalent(new[] { lobby }, m_lobbyService.Lobbies);
        }

        [Test]
        public void Test_JoinLobby_returns_null_if_the_lobby_doesnt_exist()
        {
            var lobby = m_lobbyService.JoinLobby(Guid.NewGuid(), m_client1.Channel, m_client1.User);

            Assert.IsNull(lobby);
            Assert.Collections.IsEmpty(m_lobbyService.Lobbies);
        }

        [Test]
        public void Test_JoinLobby_returns_a_lobby_instance_for_an_existing_lobby()
        {
            var lobby1 = m_lobbyService.CreateLobby(m_client1.Channel, m_client1.User);
            var lobby2 = m_lobbyService.JoinLobby(lobby1.Id, m_client2.Channel, m_client2.User);

            Assert.AreSame(lobby1, lobby2);
            Assert.Collections.Contains(m_client2.User, lobby2.Users);
            Assert.Collections.AreEquivalent(new[] { lobby1 }, m_lobbyService.Lobbies);
        }

        [Test]
        public void Test_Lobby_closes_when_last_user_leaves()
        {
            var lobby = m_lobbyService.CreateLobby(m_client1.Channel, m_client1.User);
            m_lobbyService.JoinLobby(lobby.Id, m_client2.Channel, m_client2.User);

            lobby.Logout(m_client2.Channel);
            Assert.Collections.AreEquivalent(new[] { lobby }, m_lobbyService.Lobbies);

            lobby.Logout(m_client1.Channel);
            Assert.Collections.IsEmpty(m_lobbyService.Lobbies);
        }

        [Test]
        public void Test_GetLobby_returns_the_lobby_with_the_given_id()
        {
            var lobby = m_lobbyService.CreateLobby(m_client1.Channel, m_client1.User);

            Assert.IsNotNull(lobby);
            Assert.AreEqual(lobby, m_lobbyService.GetLobby(lobby.Id));
            Assert.IsNull(m_lobbyService.GetLobby(Guid.NewGuid()));
        }

        #endregion
    }
}
