using System;
using Mox.UI.Shell;

namespace Mox.UI.Browser
{
    public class BrowseDecksCommandPartViewModel : Child
    {
        #region Methods

        public void GoBack()
        {
            var shell = this.FindParent<IShellViewModel>();
            if (shell != null)
            {
                shell.Pop();
            }
        }

        #endregion
    }
}
