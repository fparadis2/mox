using System;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public class ServerMessage : Message
    {
        public Guid User
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
