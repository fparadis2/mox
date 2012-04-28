using System;

namespace Mox.Lobby
{
    [Serializable]
    public class ServerMessage : Message
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
