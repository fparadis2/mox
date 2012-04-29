using System;
using System.Windows.Threading;
using Mox.UI.Lobby;

namespace Mox.UI.Game
{
    public class GamePageViewModel : MoxNavigationViewModel, IActivable
    {
        #region Variables

        private readonly Mox.Game m_game;
        private readonly Player m_mainPlayer;
        private readonly GameViewModel m_gameViewModel = new GameViewModel();

        private readonly PlayerInfoPartViewModel m_playerInfoPartViewModel;
        private readonly GameTablePartViewModel m_gameTablePartViewModel;
        private readonly GameBottomPartViewModel m_gameBottomPartViewModel;

        private GameViewModelSynchronizer m_synchronizer;

        #endregion

        #region Constructor

        public GamePageViewModel(LobbyViewModel lobbyViewModel, Mox.Game game, Player player)
        {
            m_game = game;
            m_mainPlayer = player;
            m_gameViewModel = new GameViewModel();

            m_playerInfoPartViewModel = ActivatePart(new PlayerInfoPartViewModel(m_gameViewModel));
            m_gameTablePartViewModel = ActivatePart(new GameTablePartViewModel(m_gameViewModel));
            m_gameBottomPartViewModel = ActivatePart(new GameBottomPartViewModel(lobbyViewModel, m_gameViewModel));
        }

        #endregion

        #region Overrides of MoxNavigationViewModel

        public override void Fill(MoxWorkspace view)
        {
            view.LeftView = m_playerInfoPartViewModel;
            view.CenterView = m_gameTablePartViewModel;
            view.RightView = null;
            view.BottomView = m_gameBottomPartViewModel;
            view.CommandView = null;
        }

        #endregion

        #region Implementation of IActivable

        public void Activate()
        {
            m_synchronizer = new GameViewModelSynchronizer(m_gameViewModel, m_game, m_mainPlayer, Dispatcher.CurrentDispatcher);
        }

        public void Deactivate()
        {
            DisposableHelper.SafeDispose(m_synchronizer);
        }

        #endregion
    }
}
