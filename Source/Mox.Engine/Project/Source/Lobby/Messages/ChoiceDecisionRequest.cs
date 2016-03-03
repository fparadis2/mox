using System;
using Mox.Flow;

namespace Mox.Lobby
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
}
