using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.UI
{
    public class DialogConductor : PropertyChangedBase
    {
        #region Variables

        private readonly object m_parent;
        private object m_dialogViewModel;

        #endregion

        #region Constructor

        public DialogConductor(object parent)
        {
            m_parent = parent;
        }

        #endregion

        #region Properties

        public object ActiveDialog
        {
            get { return m_dialogViewModel; }
            private set
            {
                if (m_dialogViewModel != value)
                {
                    m_dialogViewModel = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion

        #region Methods

        public void Push(object viewModel)
        {
            Throw.IfNull(viewModel, "viewModel");

            CloseImpl();

            IChild child = viewModel as IChild;
            if (child != null)
                child.Parent = m_parent;

            ScreenExtensions.TryActivate(viewModel);

            ActiveDialog = viewModel;
        }

        public void Close()
        {
            CloseImpl();
            ActiveDialog = null;
        }

        private void CloseImpl()
        {
            if (m_dialogViewModel != null) // Auto-close
            {
                ScreenExtensions.TryDeactivate(m_dialogViewModel, true);

                IChild child = m_dialogViewModel as IChild;
                if (child != null)
                    child.Parent = null;
            }
        }

        #endregion
    }
}
