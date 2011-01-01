using System;

namespace Mox.Lobby
{
    public interface IChatClient
    {
        /// <summary>
        /// Called when a user says something.
        /// </summary>
        /// <param name="user">The player which talked</param>
        /// <param name="message">The player message</param>
        void OnMessageReceived(User user, string message);
    }
}
