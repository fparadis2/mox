using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public class LeaderChangedMessage : Message
    {
        public Guid LeaderId { get; set; }
    }
}
