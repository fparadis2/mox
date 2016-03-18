using System;
using System.Collections.Generic;

namespace Mox.Lobby
{
    public class PlayersChangedEventArgs : EventArgs
    {
        public ChangeType Change;
        public readonly List<PlayerData> Players = new List<PlayerData>();

        public enum ChangeType
        {
            Joined,
            Left,
            Changed
        }
    }
}
