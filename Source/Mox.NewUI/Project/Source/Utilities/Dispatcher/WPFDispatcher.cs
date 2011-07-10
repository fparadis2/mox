using System.Windows.Threading;

namespace Mox.UI
{
    public class WPFDispatcher : IDispatcher
    {
        #region Variables

        private readonly Dispatcher m_dispatcher;

        #endregion

        #region Constructor

        public WPFDispatcher(Dispatcher dispatcher)
        {
            m_dispatcher = dispatcher;
        }

        #endregion

        #region Properties

        private bool InvokeRequired
        {
            get { return !m_dispatcher.CheckAccess(); }
            
        }

        #endregion

        #region Methods

        public void InvokeIfNeeded(System.Action action)
        {
            if (InvokeRequired)
            {
                m_dispatcher.Invoke(action);
            }
            else
            {
                action();
            }
        }

        public void BeginInvokeIfNeeded(System.Action action)
        {
            if (InvokeRequired)
            {
                m_dispatcher.BeginInvoke(action);
            }
            else
            {
                action();
            }
        }

        #endregion
    }
}