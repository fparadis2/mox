using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public class SetPlayerSlotDataRequest : Request<SetPlayerSlotDataResponse>
    {
        public int Index;
        public PlayerSlotDataMask Mask;
        public PlayerSlotData Data;
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
