using System;

namespace Mox.UI.Browser
{
    public class DeckListPartViewModel : Child
    {
        #region Methods

#warning temp

        public void Edit()
        {
            var conductor = this.FindParent<INavigationConductor<INavigationViewModel<MoxWorkspace>>>();
            if (conductor != null)
            {
                EditDeckPageViewModel editViewModel = new EditDeckPageViewModel();
                conductor.Push(editViewModel);
            }
        }

        #endregion
    }
}
