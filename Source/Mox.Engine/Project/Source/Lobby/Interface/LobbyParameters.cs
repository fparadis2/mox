using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Lobby
{
    public struct LobbyParameters
    {
        public IGameFormat GameFormat;
        public IDeckFormat DeckFormat;

        public bool AutoFillWithBots;
        public bool AssignNewPlayersToFreeSlots;

        public override string ToString()
        {
            if (GameFormat == null || DeckFormat == null)
                return "Unknown Format";

            return string.Format("{0} {1}", DeckFormat.Name, GameFormat.Name);
        }
    }
}
