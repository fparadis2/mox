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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Mox.Network;

namespace Mox
{
    /// <summary>
    /// Interaction logic for ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow : Window, ILog
    {
        #region Variables

        private readonly MoxHost m_host;

        #endregion

        #region Constructor

        public ServerWindow()
        {
            InitializeComponent();

            m_host = new MoxHost();
            m_host.Logs.Add(this);
        }

        #endregion

        #region Methods

        protected override void OnClosed(EventArgs e)
        {
            m_host.Close();

            base.OnClosed(e);
        }

        public void Log(LogMessage message)
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
        }

        #endregion

        #region Event Handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            m_host.Open();
        }

        #endregion
    }
}
