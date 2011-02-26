using System;
using Mox.Lobby.Backend;

namespace Mox.Lobby
{
    internal class LocalLobby : ILobby
    {
        #region Variables

        private readonly LobbyBackend m_backend;
        private readonly User m_localUser;

        #endregion

        #region Constructor

        public LocalLobby(LobbyBackend backend, User localUser)
        {
            m_backend = backend;
            m_localUser = localUser;
        }

        #endregion

        #region Properties

        public Guid Id
        {
            get { return m_backend.Id; }
        }

        public User User
        {
            get { return m_localUser; }
        }

        public IChatService Chat
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
