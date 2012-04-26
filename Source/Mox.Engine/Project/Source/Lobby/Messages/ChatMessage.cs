using System;

namespace Mox.Lobby
{
    [Serializable]
    public class ChatMessage : Message
    {
        public User User
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }
    }
}
