using System;
using System.Collections.Generic;
using Caliburn.Micro;

namespace Mox.UI
{
    public class NavigationConductor<TViewModel> : Child, INavigationConductor<TViewModel>
    {
        #region Variables

        private readonly Stack<TViewModel> m_viewModels = new Stack<TViewModel>();

        #endregion

        #region Properties

        public TViewModel ActiveItem
        {
            get
            {
                return m_viewModels.Count > 0 ? m_viewModels.Peek() : default(TViewModel);
            }
        }

        #endregion

        #region Methods

        public void Push(TViewModel viewModel)
        {
            Throw.IfNull(viewModel, "viewModel");
            m_viewModels.Push(viewModel);

            IChild child = viewModel as IChild;
            if (child != null)
            {
                child.Parent = this;
            }

            OnPush(viewModel);

            OnActiveItemChanged();
        }

        public void Pop()
        {
            var viewModel = m_viewModels.Pop();

            IChild child = viewModel as IChild;
            if (child != null)
            {
                child.Parent = null;
            }

            OnPop();

            OnActiveItemChanged();
        }

        protected virtual void OnPush(TViewModel viewModel)
        {
        }

        protected virtual void OnPop()
        {
        }

        private void OnActiveItemChanged()
        {
            NotifyOfPropertyChange(() => ActiveItem);
        }

        #endregion
    }
}
