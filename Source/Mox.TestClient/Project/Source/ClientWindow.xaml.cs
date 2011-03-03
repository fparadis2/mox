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
using System.Windows;
using System.Windows.Documents;
using Mox.Lobby;

namespace Mox
{
    /// <summary>
    /// Interaction logic for ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window, ILog
    {
        #region Variables

        private readonly Server m_server;

        #endregion

        #region Constructor

        public ClientWindow()
        {
            InitializeComponent();

            m_server = Server.CreateNetwork(this);
        }

        #endregion

        #region Methods

        protected override void OnClosed(EventArgs e)
        {
            m_server.Stop();

            base.OnClosed(e);
        }

        public void Log(LogMessage message)
        {
            Dispatcher.BeginInvoke(new System.Action(() =>
            {
                Inline content = new Run(message.Text);
                switch (message.Importance)
                {
                    case LogImportance.Error:
                    case LogImportance.Warning:
                        content = new Bold(content);
                        break;

                    default:
                        break;
                }
                logTextBox.Document.Blocks.Add(new Paragraph(content));
            }));
        }

        #endregion

        #region Event Handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            m_server.Start();
        }

        #endregion
    }
}
