using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mox.Lobby
{
    [Serializable]
    public class SetPlayerDataRequest : Message
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
    public class SetPlayerDataResponse : Message
    {
        public SetPlayerDataResult Result
        {
            get;
            set;
        }
    }
}
