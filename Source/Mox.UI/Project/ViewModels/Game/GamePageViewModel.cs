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

        private readonly DialogConductor m_dialogConductor;
        private readonly GameViewModel m_gameViewModel = new GameViewModel();
        private readonly InteractionController m_interactionController;

        private GameViewModelSynchronizer m_synchronizer;

        #endregion

        #region Constructor

        public GamePageViewModel(LobbyViewModel lobby)
        {
            m_lobby = lobby;
            m_dialogConductor = new DialogConductor(this);
            m_gameViewModel = new GameViewModel { DialogConductor = m_dialogConductor };
            m_interactionController = new InteractionController(m_gameViewModel);
        }

        #endregion

        #region Properties

        public LobbyViewModel Lobby
        {
            get { return m_lobby; }
        }

        public GameViewModel Game
        {
            get { return m_gameViewModel; }
        }

        public DialogConductor DialogConductor
        {
            get { return m_dialogConductor; }
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

                foreach (var player in m_gameViewModel.Players)
                {
                    player.LobbySlot = m_lobby.Slots[player.Source.Index];
                }
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

    internal class GamePageViewModel_DesignTime : GamePageViewModel
    {
        public GamePageViewModel_DesignTime() 
            : base(new LobbyViewModel_DesignTime())
        {
            GameViewModel_DesignTime.Initialize(Game);
        }
    }
}
