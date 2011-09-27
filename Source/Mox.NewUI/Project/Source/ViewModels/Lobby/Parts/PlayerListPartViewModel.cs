using System;
using Mox.UI.Browser;

namespace Mox.UI.Lobby
{
    public class PlayerListPartViewModel : Child
    {
        #region Variables

        private readonly LobbyViewModel m_lobbyViewModel;

        #endregion

        #region Constructor

        public PlayerListPartViewModel(LobbyViewModel lobbyViewModel)
        {
            Throw.IfNull(lobbyViewModel, "lobbyViewModel");
            m_lobbyViewModel = lobbyViewModel;
        }

        #endregion

        #region Properties

        public LobbyViewModel Lobby
        {
            get { return m_lobbyViewModel; }
        }

        #endregion
    }
}
