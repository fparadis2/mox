using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;

namespace Mox.UI.Shell
{
    public class MoxBootstrapper : Bootstrapper<ShellViewModel>
    {
        #region Variables

        private readonly IWindowManager m_windowManager = new MoxWindowManager();

        #endregion

        #region Methods

        protected override void Configure()
        {
            base.Configure();

            LogManager.GetLog = t => new CaliburnLogger();
        }

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

    public class CaliburnLogger : Caliburn.Micro.ILog
    {
        #region Constants

        private static bool LogInformation = false;

        #endregion

        #region Implementation of ILog

        public void Info(string format, params object[] args)
        {
            if (LogInformation)
            {
                Trace.TraceInformation(format, args);
            }
        }

        public void Warn(string format, params object[] args)
        {
            Trace.TraceWarning(format, args);
        }

        public void Error(Exception exception)
        {
            Trace.TraceError(exception.Message);
        }

        #endregion
    }

    public class MoxWindowManager : WindowManager
    {
        protected override Window CreateWindow(object rootModel, bool isDialog, object context)
        {
            var window = base.CreateWindow(rootModel, isDialog, context);
            window.SizeToContent = SizeToContent.Manual;
            window.MinWidth = 800;
            window.MinHeight = 600;
            TextOptions.SetTextFormattingMode(window, TextFormattingMode.Display);
            return window;
        }
    }
}
