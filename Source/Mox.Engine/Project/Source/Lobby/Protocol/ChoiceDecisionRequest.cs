using System;
using Mox.Flow;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public class ChoiceDecisionRequest : Request<ChoiceDecisionResponse>
    {
        public Choice Choice
        {
            get;
            set;
        }
    }

    [Serializable]
    public class ChoiceDecisionResponse : Response
    {
        public object Result
        {
            get;
            set;
        }
    }
}
