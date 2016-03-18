using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public class SetPlayerDataRequest : Request<SetPlayerDataResponse>
    {
        public PlayerData Data;
    }

    [Serializable]
    public class SetPlayerDataResponse : Response
    {
        public SetPlayerDataResult Result
        {
            get;
            set;
        }
    }
}
