using System;
using Mox.UI.Browser;

namespace Mox.UI.Lobby
{
    public class PlayerListPartViewModel : Child
    {
        #region Methods

#warning temp

        public void Browse()
        {
            var conductor = this.FindParent<INavigationConductor<INavigationViewModel<MoxWorkspace>>>();
            if (conductor != null)
            {
                BrowseDecksPageViewModel browseDecksPageViewModel = new BrowseDecksPageViewModel();
                conductor.Push(browseDecksPageViewModel);
            }
        }

        #endregion
    }
}
