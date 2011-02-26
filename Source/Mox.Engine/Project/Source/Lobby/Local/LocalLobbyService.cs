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
            var lobbyBackend = m_lobbyServiceBackend.CreateLobby(m_localUser);
            if (lobbyBackend != null)
            {
                return new LocalLobby(lobbyBackend, m_localUser);
            }
            return null;
        }

        public ILobby JoinLobby(Guid lobbyId)
        {
            var lobbyBackend = m_lobbyServiceBackend.JoinLobby(lobbyId, m_localUser);
            if (lobbyBackend != null)
            {
                return new LocalLobby(lobbyBackend, m_localUser);
            }
            return null;
        }

        #endregion
    }
}
