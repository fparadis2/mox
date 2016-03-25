using System;
using Mox.Lobby.Client;
using NUnit.Framework;

namespace Mox.Lobby.Server
{
    [TestFixture]
    public class LobbyServiceBackendTests
    {
        #region Variables

        private LobbyParameters m_lobbyParameters;

        private LogContext m_logContext;
        private LobbyServiceBackend m_lobbyService;

        private User m_client1;
        private User m_client2;

        private PlayerIdentity m_identity1;
        private PlayerIdentity m_identity2;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_lobbyParameters = new LobbyParameters
            {
                GameFormat = new DuelFormat(),
                DeckFormat = new StandardDeckFormat()
            };

            m_logContext = new LogContext();
            m_lobbyService = new LobbyServiceBackend(m_logContext);

            m_client1 = new User(new MockChannel(), "John");
            m_client2 = new User(new MockChannel(), "Jack");

            m_identity1 = new PlayerIdentity();
            m_identity2 = new PlayerIdentity();
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
            var lobby = m_lobbyService.CreateLobby(m_client1, m_identity1, m_lobbyParameters);

            Assert.IsNotNull(lobby);
            Assert.Collections.Contains(m_client1, lobby.Users);
            Assert.Collections.AreEquivalent(new[] { lobby }, m_lobbyService.Lobbies);
        }

        [Test]
        public void Test_JoinLobby_returns_null_if_the_lobby_doesnt_exist()
        {
            var lobby = m_lobbyService.JoinLobby(Guid.NewGuid(), m_client1, m_identity1);

            Assert.IsNull(lobby);
            Assert.Collections.IsEmpty(m_lobbyService.Lobbies);
        }

        [Test]
        public void Test_JoinLobby_returns_a_lobby_instance_for_an_existing_lobby()
        {
            var lobby1 = m_lobbyService.CreateLobby(m_client1, m_identity1, m_lobbyParameters);
            var lobby2 = m_lobbyService.JoinLobby(lobby1.Id, m_client2, m_identity2);

            Assert.AreSame(lobby1, lobby2);
            Assert.Collections.Contains(m_client2, lobby2.Users);
            Assert.Collections.AreEquivalent(new[] { lobby1 }, m_lobbyService.Lobbies);
        }

        [Test]
        public void Test_Lobby_closes_when_last_user_leaves()
        {
            var lobby = m_lobbyService.CreateLobby(m_client1, m_identity1, m_lobbyParameters);
            m_lobbyService.JoinLobby(lobby.Id, m_client2, m_identity2);

            m_lobbyService.Logout(m_client2, lobby, "gone");
            Assert.Collections.AreEquivalent(new[] { lobby }, m_lobbyService.Lobbies);

            m_lobbyService.Logout(m_client1, lobby, "gone");
            Assert.Collections.IsEmpty(m_lobbyService.Lobbies);
        }

        [Test]
        public void Test_GetLobby_returns_the_lobby_with_the_given_id()
        {
            var lobby = m_lobbyService.CreateLobby(m_client1, m_identity1, m_lobbyParameters);

            Assert.IsNotNull(lobby);
            Assert.AreEqual(lobby, m_lobbyService.GetLobby(lobby.Id));
            Assert.IsNull(m_lobbyService.GetLobby(Guid.NewGuid()));
        }

        #endregion
    }
}
