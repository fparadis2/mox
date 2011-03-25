using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Mox.UI.Shell
{
    public class MoxBootstrapper : Bootstrapper<ShellViewModel>
    {
        #region Variables

        private readonly IWindowManager m_windowManager = new WindowManager();

        #endregion

        #region Methods

        protected override object GetInstance(System.Type serviceType, string key)
        {
            if (serviceType == typeof(IWindowManager))
            {
                return m_windowManager;
            }

            return base.GetInstance(serviceType, key);
        }

        protected override IEnumerable<System.Reflection.Assembly> SelectAssemblies()
        {
            yield return typeof (MoxBootstrapper).Assembly;
            yield return typeof (ShellViewModel).Assembly;
        }

        #endregion
    }
}
