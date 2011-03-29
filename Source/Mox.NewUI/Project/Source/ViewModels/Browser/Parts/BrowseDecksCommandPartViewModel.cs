using System;

namespace Mox.UI.Browser
{
    public class BrowseDecksCommandPartViewModel : Child
    {
        #region Methods

        public void GoBack()
        {
            var shell = this.FindParent<INavigationConductor>();
            if (shell != null)
            {
                shell.Pop();
            }
        }

        #endregion
    }
}
