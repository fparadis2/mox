using Caliburn.Micro;

namespace Mox.UI
{
    public class MoxWorkspace : PropertyChangedBase
    {
        #region Variables

        private object m_leftView;
        private object m_centerView;
        private object m_rightView;
        private object m_bottomView;
        private object m_commandView;

        #endregion

        #region Properties

        public object LeftView
        {
            get { return m_leftView; }
            set
            {
                if (m_leftView != value)
                {
                    m_leftView = value;
                    NotifyOfPropertyChange(() => LeftView);
                }
            }
        }

        public object CenterView
        {
            get { return m_centerView; }
            set
            {
                if (m_centerView != value)
                {
                    m_centerView = value;
                    NotifyOfPropertyChange(() => CenterView);
                }
            }
        }

        public object RightView
        {
            get { return m_rightView; }
            set
            {
                if (m_rightView != value)
                {
                    m_rightView = value;
                    NotifyOfPropertyChange(() => RightView);
                }
            }
        }

        public object BottomView
        {
            get { return m_bottomView; }
            set
            {
                if (m_bottomView != value)
                {
                    m_bottomView = value;
                    NotifyOfPropertyChange(() => BottomView);
                }
            }
        }

        public object CommandView
        {
            get { return m_commandView; }
            set
            {
                if (m_commandView != value)
                {
                    m_commandView = value;
                    NotifyOfPropertyChange(() => CommandView);
                }
            }
        }

        #endregion
    }
}