using System;

namespace Mox.Lobby
{
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