using System;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Mox.Lobby.Client;
using Mox.UI.Game;

namespace Mox.UI.Lobby
{
    public class ConnectedPageViewModel : Conductor<object>, ICanClose
    {
        #region Variables

        private readonly Client m_client;
        private readonly LobbyViewModel m_lobbyViewModel = new LobbyViewModel();

        #endregion

        #region Constructor

        public ConnectedPageViewModel(Client client)
        {
            m_lobbyViewModel.ConnectedPageViewModel = this;

            m_client = client;
            ActivateItem(m_lobbyViewModel);
        }

        #endregion

        #region Properties

        public LobbyViewModel Lobby
        {
            get { return m_lobbyViewModel; }
        }

        #endregion

        #region Methods

        public bool CanClose()
        {
            return MessageBox.Show("Are you sure you want to disconnect from the server?", "Disconnect", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.OK;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            if (m_client != null)
            {
                m_lobbyViewModel.Bind(m_client);
                m_client.Lobby.GameService.GameStarted += WhenGameStarted;

                m_lobbyViewModel.LoadUserSettings();
            }
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);

            if (close)
            {
                m_lobbyViewModel.Dispose();

                if (m_client != null)
                {
                    m_client.Lobby.GameService.GameStarted -= WhenGameStarted;
                    m_client.Disconnect();
                }
            }
        }

        public ICommand CloseCommand
        {
            get { return new RelayCommand(Close); }
        }

        private void Close()
        {
            INavigationConductor conductor = (INavigationConductor)Parent;
            if (conductor != null)
            {
                conductor.Pop(this);
            }
        }

        private void WhenGameStarted(object sender, EventArgs e)
        {
            m_lobbyViewModel.SaveUserSettings();

            ActivateItem(new GamePageViewModel(m_lobbyViewModel));
        }

        #endregion
    }
}
