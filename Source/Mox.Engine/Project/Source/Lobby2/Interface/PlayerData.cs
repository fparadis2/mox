using System;
using Mox.Database;

namespace Mox.Lobby2
{
    [Serializable]
    public struct PlayerData
    {
        #region Properties

        public Deck Deck
        {
            get;
            set;
        }

        public bool UseRandomDeck
        {
            get; 
            set;
        }

        #endregion
    }
}
