using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mox.Lobby
{
    [Serializable]
    public class SetPlayerSlotDataRequest : Request<SetPlayerSlotDataResponse>
    {
        public int Index;
        public PlayerSlotNetworkData Data;
    }

    [Serializable]
    public class SetPlayerSlotDataResponse : Response
    {
        public SetPlayerSlotDataResult Result
        {
            get;
            set;
        }
    }
}
