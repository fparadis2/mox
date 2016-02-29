using System;
using System.Diagnostics;
using Caliburn.Micro;
using Mox.Database;
using Mox.UI.Library;
using Mox.UI.Lobby;

namespace Mox.UI.Shell
{
    public class MainHubViewModel : Conductor<object>.Collection.OneActive
    {
        private readonly HomeViewModel m_homeViewModel = new HomeViewModel();
        private readonly PlayHubViewModel m_playHubViewModel = new PlayHubViewModel();
        private readonly DeckLibraryViewModel m_deckLibraryViewModel;
        private readonly CardLibrariesViewModel m_cardLibrariesViewModel = new CardLibrariesViewModel();

        public MainHubViewModel()
        {
            m_deckLibraryViewModel = new DeckLibraryViewModel(MasterDeckLibrary.Instance);

            Items.Add(m_homeViewModel);
            Items.Add(m_playHubViewModel);
            Items.Add(m_deckLibraryViewModel);
            Items.Add(m_cardLibrariesViewModel);
        }
    }
}
