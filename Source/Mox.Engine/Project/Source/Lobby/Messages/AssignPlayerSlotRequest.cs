using System;

namespace Mox.Lobby
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
