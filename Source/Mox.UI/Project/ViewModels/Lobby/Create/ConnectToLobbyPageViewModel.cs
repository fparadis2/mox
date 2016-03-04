﻿using System;
using System.Windows.Input;
using Mox.Lobby;
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

        private readonly LobbyGameParametersViewModel m_gameParameters = new LobbyGameParametersViewModel();
        public LobbyGameParametersViewModel GameParameters
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

            client.Connect();
            client.CreateLobby(Environment.UserName, m_gameParameters.ToLobbyParameters());

            return new ConnectedPageViewModel(client);
        }

        #endregion
    }
}
