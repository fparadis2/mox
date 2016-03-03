using System;
using System.Collections.Generic;

namespace Mox.Lobby
{
    [Serializable]
    public class EnumerateLobbiesResponse : Response
    {
        public IList<Guid> Lobbies
        {
            get;
            set;
        }
    }
}