using System;
using Mox.Database;

namespace Mox.Lobby
{
    [Serializable]
    public struct PlayerData
    {
        #region Properties

        public IDeck Deck
        {
            get;
            set;
        }

        #endregion
    }
}
