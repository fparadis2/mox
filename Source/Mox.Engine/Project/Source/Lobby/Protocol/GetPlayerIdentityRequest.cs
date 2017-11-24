using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public class GetUserIdentityRequest : Request<GetUserIdentityResponse>
    {
        public Guid UserId { get; set; }
    }

    [Serializable]
    public class GetUserIdentityResponse : Response
    {
        public IUserIdentity Identity { get; set; }
    }
}
