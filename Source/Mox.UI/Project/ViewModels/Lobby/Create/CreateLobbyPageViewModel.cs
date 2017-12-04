using System;
using System.Windows.Input;
using Mox.Lobby;
using Mox.Lobby.Client;
using Mox.Lobby.Server;
using Mox.Threading;
using Caliburn.Micro;

namespace Mox.UI.Lobby
{
    public class CreateLobbyPageViewModel : Screen
    {
        #region Constructor

        public CreateLobbyPageViewModel()
        {
            DisplayName = "Create a lobby";
        }

        #endregion

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

        public string CreateText
        {
            get { return "Create Lobby"; }
        }

        #endregion

        #region Methods

        public ICommand CreateCommand
        {
            get { return new RelayCommand(Create); }
        }

        private void Create()
        {
            m_gameParameters.SaveUserSettings();

            INavigationConductor conductor = this.FindParent<INavigationConductor>();
            if (conductor != null)
            {
                conductor.Push(CreateConnectedViewModel());
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
