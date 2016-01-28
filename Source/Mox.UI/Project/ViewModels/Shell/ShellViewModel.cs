using System;
using Caliburn.Micro;

namespace Mox.UI.Shell
{
    public class ShellViewModel : Conductor<object>.Collection.OneActive
    {
        #region Variables

        private readonly MainHubViewModel m_mainHubViewModel = new MainHubViewModel();

        #endregion

        #region Constructor

        public ShellViewModel()
        {
            DisplayName = "Mox";

            ActivateItem(m_mainHubViewModel);
        }

        #endregion
    }

}
