using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mox.Lobby
{
    [Serializable]
    public class SetPlayerDataRequest : Request<SetPlayerDataResponse>
    {
        public Guid PlayerId
        {
            get;
            set;
        }

        public PlayerData PlayerData
        {
            get;
            set;
        }
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
