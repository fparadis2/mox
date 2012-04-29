using System;
using System.Collections.Generic;

namespace Mox.Lobby
{
    [Serializable]
    public class StartGameRequest : Message
    {
    }

    [Serializable]
    public class StartGameMessage : Message
    {
        public Dictionary<User, Resolvable<Mox.Player>> Players
        {
            get;
            set;
        }
    }
}
