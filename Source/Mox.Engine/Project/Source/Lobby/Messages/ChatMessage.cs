using System;

namespace Mox.Lobby
{
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
