using System;
using Mox.Lobby.Backend;

namespace Mox.Lobby
{
    internal class LocalLobbyService : ILobbyService
    {
        #region Variables

        private readonly LobbyServiceBackend m_lobbyServiceBackend;
        private readonly User m_localUser;

        #endregion

        #region Constructor

        public LocalLobbyService(LobbyServiceBackend backend, User localUser)
        {
            m_lobbyServiceBackend = backend;
            m_localUser = localUser;
        }

        #endregion

        #region Methods

        public ILobby CreateLobby()
        {
            var client = CreateClientLobby();
            var lobbyBackend = m_lobbyServiceBackend.CreateLobby(client);
            if (lobbyBackend != null)
            {
                client.Initialize(lobbyBackend);
                return client;
            }
            return null;
        }

        public ILobby JoinLobby(Guid lobbyId)
        {
            var client = CreateClientLobby();
            var lobbyBackend = m_lobbyServiceBackend.JoinLobby(lobbyId, client);
            if (lobbyBackend != null)
            {
                client.Initialize(lobbyBackend);
                return client;
            }
            return null;
        }

        private LocalLobby CreateClientLobby()
        {
            return new LocalLobby(m_localUser);
        }

        #endregion
    }
}
