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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mox.Database;
using Mox.Replication;
using Mox.UI;
using Mox.UI.Browser;

namespace Mox
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructor

        public MainWindow()
        {
            MasterCardDatabase.BeginLoading();
            GameFlow.Navigated += GameFlow_Navigated;
            Loaded += MainWindow_Loaded;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            GameFlow.Navigated -= GameFlow_Navigated;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GameFlow.GoToPage<BrowseDecksPage>();
        }

        void GameFlow_Navigated(object sender, GameFlowNavigationEventArgs e)
        {
            frame.Content = e.Content;
        }

        #endregion
    }
}
