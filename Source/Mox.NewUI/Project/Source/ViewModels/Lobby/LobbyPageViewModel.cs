using System;

namespace Mox.UI.Lobby
{
    public class LobbyPageViewModel : MoxNavigationViewModel
    {
        #region Variables

        private readonly PlayerListPartViewModel m_players;
        private readonly GameInfoPartViewModel m_gameInfo;
        private readonly LobbyChatPartViewModel m_chat;
        private readonly LobbyCommandPartViewModel m_command;

        #endregion

        #region Constructor

        public LobbyPageViewModel()
        {
            m_players = ActivatePart(new PlayerListPartViewModel());
            m_gameInfo = ActivatePart(new GameInfoPartViewModel());
            m_chat = ActivatePart(new LobbyChatPartViewModel());
            m_command = ActivatePart(new LobbyCommandPartViewModel());
        }

        #endregion

        #region Methods

        public override void Fill(MoxWorkspace view)
        {
            view.LeftView = null;
            view.CenterView = m_players;
            view.RightView = m_gameInfo;
            view.BottomView = m_chat;
            view.CommandView = m_command;
        }

        #endregion
    }
}
