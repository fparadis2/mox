using System;
using Mox.UI.Game;
using Mox.UI.Shell;

namespace Mox.UI.Lobby
{
    public class LobbyCommandPartViewModel : Child
    {
        #region Methods

        public void LeaveGame()
        {
            var conductor = this.FindParent<INavigationConductor>();
            if (conductor != null)
            {
                conductor.Pop();
            }
        }

        public void StartGame()
        {
            var shell = this.FindParent<IShellViewModel>();
            if (shell != null)
            {
                shell.Push(new GamePageViewModel());
            }
        }

        #endregion
    }
}
