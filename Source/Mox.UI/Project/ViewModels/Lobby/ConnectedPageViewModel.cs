using System;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Mox.Lobby.Client;

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
            m_client = client;

            if (client != null)
            {
                m_serverName = client.ServerName;
                m_lobbyViewModel.Bind(client.Lobby);
            }
        }

        #endregion

        #region Properties

        public LobbyViewModel Lobby
        {
            get { return m_lobbyViewModel; }
        }

        private string m_serverName;

        public string ServerName
        {
            get { return m_serverName; }
            set
            {
                if (m_serverName != value)
                {
                    m_serverName = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion

        #region Methods

        public bool CanClose()
        {
            return MessageBox.Show("Are you sure you want to disconnect from the server?", "Disconnect", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.OK;
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);

            if (close)
            {
                if (m_client != null)
                    m_client.Disconnect();

                m_lobbyViewModel.Dispose();
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

        #endregion
    }

    public class ConnectedPageViewModel_DesignTime : ConnectedPageViewModel
    {
        public ConnectedPageViewModel_DesignTime() 
            : base(null)
        {
            ServerName = "My Server";
        }
    }
}
