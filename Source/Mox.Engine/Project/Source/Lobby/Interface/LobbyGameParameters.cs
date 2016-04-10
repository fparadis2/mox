using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Lobby
{
    [Serializable]
    public struct LobbyGameParameters
    {
        public LobbyGameBotParameters BotParameters;

        public void SetAsDefault()
        {
            BotParameters.SetAsDefault();
        }
    }

    [Serializable]
    public struct LobbyGameBotParameters
    {
        public LobbyGameAIType AIType;
        public double TimeOut;

        public void SetAsDefault()
        {
            AIType = LobbyGameAIType.MinMax;
            TimeOut = 10;
        }
    }

    public enum LobbyGameAIType
    {
        Dead,
        Random,
        MinMax
    }
}
