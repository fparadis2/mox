// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Linq;
using System.Windows;
using Mox.Lobby;

namespace Mox
{
    /// <summary>
    /// Interaction logic for ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow
    {
        #region Variables

        private readonly Client m_client;
        private ClientViewModel m_viewModel;

        #endregion

        #region Constructor

        public ClientWindow()
        {
            InitializeComponent();

            m_client = Client.CreateNetwork();
        }

        #endregion

        #region Methods

        protected override void OnClosed(EventArgs e)
        {
            m_client.Disconnect();

            base.OnClosed(e);
        }

        #endregion

        #region Event Handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            m_client.Disconnected += m_client_Disconnected; 
            m_client.Connect();

            var lobbies = m_client.GetLobbies();
            if (lobbies.Any())
            {
                m_client.EnterLobby(lobbies.First(), "Second guy");
            }
            else
            {
                m_client.CreateLobby("First guy");
            }

            m_viewModel = new ClientViewModel(m_client.Lobby);
            DataContext = m_viewModel;
        }

        void Window_Closed(object sender, EventArgs e)
        {
            m_client.Disconnect();
        }

        void m_client_Disconnected(object sender, EventArgs e)
        {
            m_viewModel.OnDisconnected();
        }

        #endregion
    }
}
