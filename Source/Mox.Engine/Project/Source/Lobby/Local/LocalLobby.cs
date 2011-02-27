using System;
using Mox.Lobby.Backend;

namespace Mox.Lobby
{
    internal class LocalLobby : ILobby, IClient
    {
        #region Variables
        
        private readonly User m_localUser;

        private readonly LocalChatService m_chatService;

        private LobbyBackend m_backend;

        #endregion

        #region Constructor

        public LocalLobby(User localUser)
        {
            m_localUser = localUser;

            m_chatService = new LocalChatService(this);
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
            get { return m_chatService; }
        }

        internal LobbyBackend Backend
        {
            get { return m_backend; }
        }

        #endregion

        #region Methods

        internal void Initialize(LobbyBackend backend)
        {
            m_backend = backend;
        }

        #endregion

        #region IClient

        IChatClient IClient.ChatClient
        {
            get { return m_chatService; }
        }

        #endregion
    }
}
