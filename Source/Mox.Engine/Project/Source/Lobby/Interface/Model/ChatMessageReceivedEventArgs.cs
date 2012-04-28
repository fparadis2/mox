using System;

namespace Mox.Lobby
{
    public class ChatMessageReceivedEventArgs : EventArgs
    {
        public ChatMessageReceivedEventArgs(User user, string msg)
        {
            User = user;
            Message = msg;
        }

        /// <summary>
        /// The player which talked.
        /// </summary>
        public User User { get; private set; }

        /// <summary>
        /// The player message.
        /// </summary>
        public string Message { get; private set; }

        public string ToChatMessage()
        {
            string userName = User == null ? "[[Unknown]]" : User.Name;
            return string.Format("{0}: {1}", userName, Message);
        }
    }
}