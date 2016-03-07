using System;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public class AssignPlayerSlotRequest : Request<AssignPlayerSlotResponse>
    {
        public int Index;
        public Guid User;
    }

    [Serializable]
    public class AssignPlayerSlotResponse : Response
    {
        public AssignPlayerSlotResult Result
        {
            get;
            set;
        }
    }
}
