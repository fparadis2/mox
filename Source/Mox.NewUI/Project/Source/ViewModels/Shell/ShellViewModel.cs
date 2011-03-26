using System;
using Caliburn.Micro;

namespace Mox.UI.Shell
{
    public class ShellViewModel : Conductor<object>
    {
        #region Constructor

        public ShellViewModel()
        {
            ActivateItem(new MainMenuViewModel());
        }

        #endregion

        #region Properties

        public override string DisplayName
        {
            get { return "Mox"; }
            set { }
        }

        #endregion
    }

}
