using System;
using Mox.Database;

namespace Mox.Lobby
{
    [Serializable]
    public struct PlayerData
    {
        #region Properties

        public string Deck
        {
            get;
            set;
        }

        #endregion
    }
}
