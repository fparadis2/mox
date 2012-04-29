using System;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    public class LobbyCommandPartViewModel : Child
    {
        #region Variables

        private readonly ILobby m_lobby;
        private bool m_canStartGame = true;

        #endregion

        #region Constructor

        public LobbyCommandPartViewModel(ILobby lobby)
        {
            m_lobby = lobby;
        }

        #endregion

        #region Methods

        public void LeaveGame()
        {
            var conductor = this.FindParent<INavigationConductor>();
            if (conductor != null)
            {
                conductor.Pop();
            }
        }

        public bool CanStartGame()
        {
            return m_canStartGame;
        }

        public void StartGame()
        {
            m_canStartGame = false;
            m_lobby.GameService.StartGame();
        }

        #endregion
    }
}
