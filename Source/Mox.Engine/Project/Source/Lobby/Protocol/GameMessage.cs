using System;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public class GameMessage : Message
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
