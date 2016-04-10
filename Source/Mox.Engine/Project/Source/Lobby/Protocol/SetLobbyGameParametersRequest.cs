using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public class SetLobbyGameParametersRequest : Request<SetLobbyGameParametersResponse>
    {
        public LobbyGameParameters Parameters;
    }

    [Serializable]
    public class SetLobbyGameParametersResponse : Response
    {
        public SetLobbyGameParametersResponse(bool result)
        {
            Result = result;
        }

        public bool Result
        {
            get;
            set;
        }
    }
}
