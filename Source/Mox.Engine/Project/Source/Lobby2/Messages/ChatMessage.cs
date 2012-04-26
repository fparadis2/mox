using System;

namespace Mox.Lobby2
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
