using System;
using System.Windows.Input;
using Mox.Lobby;
using Mox.Lobby.Client;
using Mox.Lobby.Server;
using Mox.Threading;

namespace Mox.UI.Lobby
{
    public class ConnectToLobbyPageViewModel : PageViewModel
    {
        #region Properties

        private readonly LobbyServerParametersViewModel m_serverParameters = new LobbyServerParametersViewModel();
        public LobbyServerParametersViewModel ServerParameters
        {
            get { return m_serverParameters; }
        }

        private readonly LobbyParametersViewModel m_gameParameters = new LobbyParametersViewModel();
        public LobbyParametersViewModel GameParameters
        {
            get { return m_gameParameters; }
        }

        public string ConnectText
        {
            get { return "Create Lobby"; }
        }

        #endregion

        #region Methods

        public ICommand ConnectCommand
        {
            get { return new RelayCommand(Connect); }
        }

        private void Connect()
        {
            m_gameParameters.SaveUserSettings();

            INavigationConductor conductor = (INavigationConductor)Parent;
            if (conductor != null)
            {
                conductor.TransitionTo(CreateConnectedViewModel());
            }
        }

        private ConnectedPageViewModel CreateConnectedViewModel()
        {
            var server = Server.CreateLocal(new LogContext());
            var client = Client.CreateLocal(server);
            client.Dispatcher = WPFDispatcher.FromCurrentThread();

            var localIdentity = PlayerIdentityRepository.Local;

            client.Connect();
            client.CreateLobby(localIdentity, m_gameParameters.ToLobbyParameters());

            return new ConnectedPageViewModel(client);
        }

        #endregion
    }
}
