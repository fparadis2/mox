using System;

namespace Mox.UI.Shell
{
    public class ShellViewModel : NavigationConductor
    {
        #region Variables

        private readonly MainHubViewModel m_mainHubViewModel = new MainHubViewModel();

        #endregion

        #region Constructor

        public ShellViewModel()
        {
            DisplayName = "Mox";

            Push(m_mainHubViewModel);
        }

        #endregion
    }

}
