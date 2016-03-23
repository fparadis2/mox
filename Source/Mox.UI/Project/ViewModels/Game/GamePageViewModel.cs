using System;
using System.Windows.Threading;
using Caliburn.Micro;
using Mox.Lobby;
using Mox.UI.Lobby;

namespace Mox.UI.Game
{
    public class GamePageViewModel : Screen
    {
        #region Variables

        private readonly LobbyViewModel m_lobby;
        private readonly GameViewModel m_gameViewModel = new GameViewModel();
        private readonly InteractionController m_interactionController;

        private GameViewModelSynchronizer m_synchronizer;

        #endregion

        #region Constructor

        public GamePageViewModel(LobbyViewModel lobby)
        {
            m_lobby = lobby;
            m_gameViewModel = new GameViewModel();
            m_interactionController = new InteractionController(m_gameViewModel);
        }

        #endregion

        #region Lifetime

        protected override void OnActivate()
        {
            base.OnActivate();

            if (m_lobby.Source != null)
            {
                var gameService = m_lobby.Source.GameService;

                m_synchronizer = new GameViewModelSynchronizer(m_gameViewModel, gameService.Game, gameService.Player, Dispatcher.CurrentDispatcher);
                gameService.InteractionRequested += GameService_InteractionRequested;
            }
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);

            if (close)
            {
                var gameService = m_lobby.Source.GameService;
                gameService.InteractionRequested -= GameService_InteractionRequested;

                DisposableHelper.SafeDispose(m_synchronizer);
            }
        }

        #endregion

        #region Event Handlers

        void GameService_InteractionRequested(object sender, InteractionRequestedEventArgs e)
        {
            m_interactionController.BeginInteraction(e);
        }

        #endregion
    }
}
