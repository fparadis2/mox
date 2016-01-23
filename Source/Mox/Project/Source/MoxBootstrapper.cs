using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Mono.Options;
using Mox.Database;

namespace Mox.UI.Shell
{
    public class MoxBootstrapper : BootstrapperBase
    {
        #region Variables

        private readonly ShellViewModel m_shellViewModel = new ShellViewModel();
        private readonly IWindowManager m_windowManager = new MoxWindowManager();

        #endregion

        #region Constructor

        public MoxBootstrapper()
        {
            Initialize();
        }

        #endregion

        #region Methods

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);

            m_windowManager.ShowWindow(m_shellViewModel);
        }

        protected override void Configure()
        {
            base.Configure();

            MasterCardDatabase.BeginLoading();
            //ViewModelDataSource.UseRealSource();

            LogManager.GetLog = t => new CaliburnLogger();
        }

        protected override object GetInstance(System.Type serviceType, string key)
        {
            if (serviceType == typeof(IWindowManager))
            {
                return m_windowManager;
            }
            
            if (serviceType == typeof(ShellViewModel))
            {
                return m_shellViewModel;
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

    internal class CaliburnLogger : Caliburn.Micro.ILog
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

    internal class MoxWindowManager : MetroWindowManager
    {
        public override void ConfigureWindow(Window window)
        {
            base.ConfigureWindow(window);

            window.SizeToContent = SizeToContent.Manual;
            window.MinWidth = 800;
            window.MinHeight = 600;
            window.UseLayoutRounding = true;
            TextOptions.SetTextFormattingMode(window, TextFormattingMode.Display);
        }
    }
}
