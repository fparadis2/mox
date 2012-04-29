using System;

namespace Mox.Lobby
{
    public interface IGameService
    {
        #region Properties

        Game Game { get; }
        Mox.Player Player { get; }

        #endregion

        #region Methods

        void StartGame();

        #endregion

        #region Events

        event EventHandler GameStarted;

        #endregion
    }
}
