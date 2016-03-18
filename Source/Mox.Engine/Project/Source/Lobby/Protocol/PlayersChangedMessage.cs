using System;
using System.Collections.Generic;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public class PlayersChangedMessage : Message
    {
        #region Properties

        public ChangeType Change;
        public readonly List<PlayerData> Players = new List<PlayerData>();

        #endregion

        #region Constructor

        public PlayersChangedMessage(ChangeType change, IEnumerable<PlayerData> players)
        {
            Change = change;
            Players.AddRange(players);
        }

        #endregion

        #region Nested Types

        public enum ChangeType
        {
            Joined,
            Left,
            Changed
        }

        #endregion
    }
}
