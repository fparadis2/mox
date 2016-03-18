using System;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public class ChatMessage : Message
    {
        public Guid SpeakerId;
        public string Message;
    }
}
