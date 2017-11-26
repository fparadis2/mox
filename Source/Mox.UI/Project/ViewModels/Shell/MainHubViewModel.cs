using System;
using System.Diagnostics;
using Caliburn.Micro;
using Mox.Database;
using Mox.UI.Library;
using Mox.UI.Lobby;
using System.Collections.Generic;

namespace Mox.UI.Shell
{
    public class MainHubViewModel : Conductor<object>.Collection.OneActive
    {
        private readonly HomeViewModel m_homeViewModel = new HomeViewModel();
        private readonly PlayHubViewModel m_playHubViewModel = new PlayHubViewModel();
        private readonly DeckLibraryViewModel m_deckLibraryViewModel;
        private readonly CardLibrariesViewModel m_cardLibrariesViewModel = new CardLibrariesViewModel();

        public Screen Home { get { return m_homeViewModel; } }
        public Screen Play { get { return m_playHubViewModel; } }
        public Screen Decks { get { return m_deckLibraryViewModel; } }
        public Screen Cards { get { return m_cardLibrariesViewModel; } }

        public MainHubViewModel()
        {
            m_deckLibraryViewModel = new DeckLibraryViewModel(MasterDeckLibrary.Instance);

            ActivateItem(m_homeViewModel);
        }

        public void ActivateHubItem(object item)
        {
            ActivateItem(item);
        }
    }
}
