using System;

namespace Mox.UI.Browser
{
    public class EditDeckCommandPartViewModel : Child
    {
        #region Methods

        public void Cancel()
        {
            var shell = this.FindParent<INavigationConductor>();
            if (shell != null)
            {
                shell.Pop();
            }
        }

        public void Save()
        {
#warning TODO

            var shell = this.FindParent<INavigationConductor>();
            if (shell != null)
            {
                shell.Pop();
            }
        }

        #endregion
    }
}
