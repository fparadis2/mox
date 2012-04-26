using System;
using Mox.Database;

namespace Mox.Lobby2
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
