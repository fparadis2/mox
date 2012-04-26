using System;

namespace Mox.Lobby
{
    public interface IChatService
    {
        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="msg"></param>
        void Say(string msg);

        /// <summary>
        /// Called when a user says something.
        /// </summary>
        event EventHandler<ChatMessageReceivedEventArgs> MessageReceived;
    }
}
