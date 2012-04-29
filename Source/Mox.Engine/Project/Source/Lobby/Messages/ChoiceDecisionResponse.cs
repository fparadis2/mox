using System;

namespace Mox.Lobby
{
    [Serializable]
    public class ChoiceDecisionResponse : Message
    {
        public object Result
        {
            get;
            set;
        }
    }
}