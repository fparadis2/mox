using System;
using Mox.UI.Lobby;

namespace Mox.UI.Game
{
    public class GameBottomPartViewModel
    {
        #region Variables

        private readonly LobbyViewModel m_lobbyViewModel;
        private readonly GameViewModel m_gameViewModel;

        #endregion

        #region Constructor

        public GameBottomPartViewModel(LobbyViewModel lobbyViewModel, GameViewModel gameViewModel)
        {
            m_lobbyViewModel = lobbyViewModel;
            m_gameViewModel = gameViewModel;
        }

        #endregion

        #region Properties

        public GameViewModel Game
        {
            get { return m_gameViewModel; }
        }

        public LobbyViewModel Lobby
        {
            get { return m_lobbyViewModel; }
        }

        #endregion
    }
}
