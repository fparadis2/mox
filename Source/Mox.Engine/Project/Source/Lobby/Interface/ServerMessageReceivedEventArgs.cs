using System;

namespace Mox.Lobby
{
    public class ServerMessageReceivedEventArgs : EventArgs
    {
        public ServerMessageReceivedEventArgs(string msg)
        {
            Message = msg;
        }

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