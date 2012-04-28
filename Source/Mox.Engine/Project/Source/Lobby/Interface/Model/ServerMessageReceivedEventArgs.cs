using System;

namespace Mox.Lobby
{
    public class ServerMessageReceivedEventArgs : EventArgs
    {
        public ServerMessageReceivedEventArgs(User user, string msg)
        {
            User = user;
            Message = msg;
        }

        /// <summary>
        /// The user that the message is about, if any.
        /// </summary>
        public User User { get; private set; }

        /// <summary>
        /// The message.
        /// </summary>
        public string Message { get; private set; }

        public string ToServerMessage()
        {
            return string.Concat("[", Message, "]");
        }
    }
}