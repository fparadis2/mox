using System;

namespace Mox.Lobby
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(User user, string msg)
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
    }
}