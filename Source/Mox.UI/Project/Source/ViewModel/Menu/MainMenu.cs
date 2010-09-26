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
using System.Windows.Controls;
using System.Windows.Navigation;
using Mox.UI.Browser;

namespace Mox.UI
{
    public class MainMenu : MainMenuItemModel
    {
        #region Constructor

        public MainMenu()
            : base(null)
        {
            var playMenu = Create();
            playMenu.Text = "Play";
            Items.Add(playMenu);
            {
                var singlePlayer = Create(GameFlow.Instance.GoToPage<GamePage>);
                singlePlayer.Text = "Single Player";
                playMenu.Items.Add(singlePlayer);
            }

            var browseMenu = Create();
            browseMenu.Text = "Browse";
            Items.Add(browseMenu);
            {
                var browseCards = Create(GameFlow.Instance.PushPage<CardBrowserPage>);
                browseCards.Text = "All Cards";
                browseMenu.Items.Add(browseCards);

                var browseDecks = Create(GameFlow.Instance.PushPage<BrowseDecksPage>);
                browseDecks.Text = "Decks";
                browseMenu.Items.Add(browseDecks);
            }

            var quitMenu = Create(() => Application.Current.Shutdown());
            quitMenu.Text = "Exit";
            Items.Add(quitMenu);
        }

        #endregion
    }

    internal class DesignTimeMainMenu : MainMenu
    {
        public DesignTimeMainMenu()
        {
            SelectedItem = Items[1]; // Browse
        }
    }
}
