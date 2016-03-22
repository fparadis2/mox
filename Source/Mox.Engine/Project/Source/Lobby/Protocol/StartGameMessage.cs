using System;
using System.Collections.Generic;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public class StartGameRequest : Request<StartGameResponse>
    {
    }

    public class StartGameResponse : Response
    {
        public bool Result { get; set; }
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
    public class GameStartedMessage : Message
    {
    }
}
