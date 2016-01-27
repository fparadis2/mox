using System;
using System.Diagnostics;
using Caliburn.Micro;
using Mox.UI.Library;

namespace Mox.UI.Shell
{
    public class MainHubViewModel : Conductor<IMoxScreen>.Collection.OneActive, IMoxScreen
    {
        private readonly HomeViewModel m_homeViewModel = new HomeViewModel();
        private readonly PlayHubViewModel m_playHubViewModel = new PlayHubViewModel();
        private readonly DeckLibrariesViewModel m_deckLibrariesViewModel = new DeckLibrariesViewModel();
        private readonly CardLibrariesViewModel m_cardLibrariesViewModel = new CardLibrariesViewModel();

        public MainHubViewModel()
        {
            Items.Add(m_homeViewModel);
            Items.Add(m_playHubViewModel);
            Items.Add(m_deckLibrariesViewModel);
            Items.Add(m_cardLibrariesViewModel);
        }

        public void Goto()
        {
            this.ActivateScreen();
        }
    }
}
