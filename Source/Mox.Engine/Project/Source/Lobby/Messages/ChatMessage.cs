using System;

namespace Mox.Lobby
{
    [Serializable]
    public class ChatMessage : Message
    {
        public Guid Speaker;
        public string Message;
    }
}
