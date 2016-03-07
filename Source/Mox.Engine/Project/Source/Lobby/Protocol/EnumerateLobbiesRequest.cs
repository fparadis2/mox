using System;
using System.Collections.Generic;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public class EnumerateLobbiesRequest : Request<EnumerateLobbiesResponse>
    {}

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