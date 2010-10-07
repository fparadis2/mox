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
using Mox.Database;
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
            GameFlow.Instance.Navigated += GameFlow_Navigated;
            Loaded += MainWindow_Loaded;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            GameFlow.Instance.Navigated -= GameFlow_Navigated;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GameFlow.Instance.GoToPage<BrowseDecksPage>();
        }

        void GameFlow_Navigated(object sender, GameFlowNavigationEventArgs e)
        {
            frame.Content = e.Content;
        }

        #endregion
    }
}
