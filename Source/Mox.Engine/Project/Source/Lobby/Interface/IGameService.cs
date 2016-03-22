using System;
using System.Threading.Tasks;

namespace Mox.Lobby
{
    public interface IGameService
    {
        #region Properties

        bool IsStarted { get; }
        Game Game { get; }
        Player Player { get; }

        #endregion

        #region Methods

        Task<bool> StartGame();

        #endregion

        #region Events

        event EventHandler GameStarted;
        event EventHandler<InteractionRequestedEventArgs> InteractionRequested;

        #endregion
    }
}
