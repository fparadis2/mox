using System;

namespace Mox.Lobby
{
    [Serializable]
    public sealed class GameInfo
    {
        #region Constructor

        public GameInfo()
        {
            NumberOfPlayers = 2;
        }

        private GameInfo(GameInfo gameInfo)
        {
            NumberOfPlayers = gameInfo.NumberOfPlayers;
        }

        #endregion

        #region Properties

        public int NumberOfPlayers
        {
            get;
            set;
        }

        #endregion

        #region Methods

        public GameInfo Clone()
        {
            return new GameInfo(this);
        }

        #endregion
    }
}
