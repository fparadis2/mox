using System;
using Mox.Flow;

namespace Mox.Lobby
{
    [Serializable]
    public class ChoiceDecisionRequest : Message
    {
        public Choice Choice
        {
            get;
            set;
        }
    }
}
