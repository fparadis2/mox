using System;
using System.Collections.Generic;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public class StartGameRequest : Message
    {
    }

    [Serializable]
    public class PrepareGameMessage : Message
    {
        public Dictionary<Guid, Resolvable<Player>> Players
        {
            get;
            set;
        }
    }

    [Serializable]
    public class StartGameMessage : Message
    {
    }
}
