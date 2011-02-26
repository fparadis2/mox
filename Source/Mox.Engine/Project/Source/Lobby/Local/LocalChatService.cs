using System;

using Mox.Lobby.Backend;

namespace Mox.Lobby
{
    internal class LocalChatService : LocalService, IChatService, IChatClient
    {
        #region Variables

        private readonly ChatServiceBackend m_backend = new ChatServiceBackend();

        #endregion

        #region Constructor

        public LocalChatService()
        {
#warning Todo: pass correct chat level? Is this the correct place for registering?
            m_backend.Register(User, this, ChatLevel.Normal);
        }

        #endregion

        #region Methods

        public void Say(string msg)
        {
            m_backend.Say(User, msg);
        }

        #endregion

        #region Events

        void IChatClient.OnMessageReceived(MessageReceivedEventArgs e)
        {
            MessageReceived.Raise(this, e);
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        #endregion
    }
}
