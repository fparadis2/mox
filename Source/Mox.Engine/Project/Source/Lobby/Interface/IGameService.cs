using System;

namespace Mox.Lobby
{
    public interface IGameService
    {
        #region Properties

        Game Game { get; }

        #endregion

        #region Events

        event EventHandler GameStarted;

        #endregion
    }
}
