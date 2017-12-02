using System;
using Caliburn.Micro;
using Mox.UI.Library;
using Mox.UI.Lobby;

namespace Mox.UI.Shell
{
    public class MainHubViewModel : Conductor<object>.Collection.OneActive
    {
        private readonly HomeViewModel m_homeViewModel = new HomeViewModel();
        private readonly PlayHubViewModel m_playHubViewModel = new PlayHubViewModel();
        private readonly DecksViewModel m_decksViewModel = new DecksViewModel();
        private readonly CardLibrariesViewModel m_cardLibrariesViewModel = new CardLibrariesViewModel();

        public Screen Home { get { return m_homeViewModel; } }
        public Screen Play { get { return m_playHubViewModel; } }
        public Screen Decks { get { return m_decksViewModel; } }
        public Screen Cards { get { return m_cardLibrariesViewModel; } }

        public MainHubViewModel()
        {
            ActivateItem(m_homeViewModel);
        }

        public void ActivateHubItem(object item)
        {
            ActivateItem(item);
        }
    }
}
