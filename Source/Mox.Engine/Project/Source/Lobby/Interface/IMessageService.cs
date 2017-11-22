using System;

namespace Mox.Lobby
{
    public interface IMessageService
    {
        /// <summary>
        /// Sends a chat message.
        /// </summary>
        /// <param name="msg"></param>
        void SendMessage(string msg);

        /// <summary>
        /// Called when a user says something.
        /// </summary>
        event EventHandler<ChatMessage> ChatMessageReceived;

        /// <summary>
        /// Called when the server says something.
        /// </summary>
        event EventHandler<ServerMessage> ServerMessageReceived;

        /// <summary>
        /// Called when the game says something.
        /// </summary>
        event EventHandler<GameMessage> GameMessageReceived;
    }

    public struct ChatMessage
    {
        public Guid SpeakerUserId;
        public string Text;
    }

    public struct ServerMessage
    {
        public Guid UserId;
        public string Text;
    }

    public struct GameMessage
    {
        public Guid SourceUserId;
        public string Text;
    }
}
