namespace Mox.Lobby
{
    public interface IChatService
    {
        /// <summary>
        /// Makes the given client say something to other clients.
        /// </summary>
        /// <param name="user">The user that says the message</param>
        /// <param name="message">The user message</param>
        void Say(User user, string message);
    }
}