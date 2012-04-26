using System;
using System.Collections.Generic;

namespace Mox.Lobby
{
    [Serializable]
    public class EnumerateLobbiesResponse : Message
    {
        public IEnumerable<Guid> Lobbies
        {
            get;
            set;
        }
    }
}