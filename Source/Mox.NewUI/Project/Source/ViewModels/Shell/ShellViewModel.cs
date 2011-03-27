using System;
using Caliburn.Micro;

namespace Mox.UI.Shell
{
    public class ShellViewModel : NavigationConductor<object>, IShellViewModel, IHaveDisplayName
    {
        #region Constructor

        public ShellViewModel()
        {
            Push(new MainMenuViewModel(this));
        }

        #endregion

        #region Properties

        public string DisplayName
        {
            get { return "Mox"; }
            set { }
        }

        #endregion
    }

}
