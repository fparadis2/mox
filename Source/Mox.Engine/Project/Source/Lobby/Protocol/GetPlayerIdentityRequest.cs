using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public class GetPlayerIdentityRequest : Request<GetPlayerIdentityResponse>
    {
        public Guid PlayerId { get; set; }
    }

    [Serializable]
    public class GetPlayerIdentityResponse : Response
    {
        public IPlayerIdentity Identity { get; set; }
    }
}
