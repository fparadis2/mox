using System;

using Mox.Lobby.Backend;

namespace Mox.Lobby
{
    internal class LocalChatService : LocalService, IChatService, IChatClient
    {
        #region Constructor

        public LocalChatService(LocalLobby owner)
            : base(owner)
        {
        }

        #endregion

        #region Properties

        private ChatServiceBackend Backend
        {
            get { return Owner.Backend.ChatService; }
        }

        #endregion

        #region Methods

        public void Say(string msg)
        {
            Backend.Say(User, msg);
        }

        #endregion

        #region Events

        void IChatClient.OnMessageReceived(User user, string message)
        {
            MessageReceived.Raise(this, new MessageReceivedEventArgs(user, message));
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        #endregion
    }
}
