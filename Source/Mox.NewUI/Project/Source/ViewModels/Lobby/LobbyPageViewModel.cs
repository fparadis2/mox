using System;
using System.Diagnostics;
using Mox.Lobby;
using Mox.UI.Game;

namespace Mox.UI.Lobby
{
    public class LobbyPageViewModel : MoxNavigationViewModel, IActivable
    {
        #region Variables

        private readonly ILobby m_lobby;
        private readonly LobbyViewModel m_lobbyViewModel = new LobbyViewModel();
        private LobbyViewModelSynchronizer m_lobbyViewModelSynchronizer;

        private readonly PlayerListPartViewModel m_players;
        private readonly GameInfoPartViewModel m_gameInfo;
        private readonly LobbyChatPartViewModel m_chat;
        private readonly LobbyCommandPartViewModel m_command;

        #endregion

        #region Constructor

        public LobbyPageViewModel(ILobby lobby)
        {
            m_lobby = lobby;

            m_players = ActivatePart(new PlayerListPartViewModel(m_lobbyViewModel));
            m_gameInfo = ActivatePart(new GameInfoPartViewModel());
            m_chat = ActivatePart(new LobbyChatPartViewModel(m_lobbyViewModel));
            m_command = ActivatePart(new LobbyCommandPartViewModel(m_lobby));
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

        #region Implementation of IActivable

        public void Activate()
        {
            Debug.Assert(m_lobbyViewModelSynchronizer == null, "Not supposed to activate twice.");

            m_lobbyViewModelSynchronizer = new LobbyViewModelSynchronizer(m_lobbyViewModel, m_lobby);
            m_lobby.GameService.GameStarted += GameService_GameStarted;
        }

        public virtual void Deactivate()
        {
            m_lobby.GameService.GameStarted -= GameService_GameStarted;
            m_lobbyViewModelSynchronizer.Dispose();
        }

        #endregion

        #region Event Handlers

        void GameService_GameStarted(object sender, EventArgs e)
        {
            var shell = this.FindParent<INavigationConductor<object>>();
            if (shell != null)
            {
                shell.Push(new GamePageViewModel(m_lobby, m_lobbyViewModel, m_lobby.GameService.Game, m_lobby.GameService.Player));
            }
        }

        #endregion
    }
}
