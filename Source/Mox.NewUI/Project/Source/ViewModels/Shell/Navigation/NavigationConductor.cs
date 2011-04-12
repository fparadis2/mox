﻿using System;
using System.Collections.Generic;
using Caliburn.Micro;

namespace Mox.UI
{
    public class NavigationConductor<TViewModel> : Child, INavigationConductor<TViewModel>
    {
        #region Variables

        private readonly Stack<PageHolder> m_viewModels = new Stack<PageHolder>();

        #endregion

        #region Properties

        public TViewModel ActiveItem
        {
            get
            {
                return m_viewModels.Count > 0 ? m_viewModels.Peek().ViewModel : default(TViewModel);
            }
        }

        #endregion

        #region Methods

        public IPageHandle Push(TViewModel viewModel)
        {
            Throw.IfNull(viewModel, "viewModel");
            var pageHandle = new PageHolder(viewModel);
            m_viewModels.Push(pageHandle);

            IChild child = viewModel as IChild;
            if (child != null)
            {
                child.Parent = this;
            }

            OnPush(viewModel);

            OnActiveItemChanged();
            return pageHandle;
        }

        public void Pop()
        {
            if (m_viewModels.Count == 1)
            {
                if (TryPopParent())
                {
                    return;
                }
            }

            var pageHolder = m_viewModels.Pop();
            var viewModel = pageHolder.ViewModel;

            IChild child = viewModel as IChild;
            if (child != null)
            {
                child.Parent = null;
            }

            OnPop();

            pageHolder.OnClosed(EventArgs.Empty);
            OnActiveItemChanged();
        }

        private bool TryPopParent()
        {
            INavigationConductor parentConductor = Parent as INavigationConductor;
            if (parentConductor != null)
            {
                parentConductor.Pop();
                return true;
            }

            return false;
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

        #region Inner Types

        private class PageHolder : IPageHandle
        {
            #region Variables

            private readonly TViewModel m_viewModel;

            #endregion

            #region Constructor

            public PageHolder(TViewModel viewModel)
            {
                m_viewModel = viewModel;
            }

            #endregion

            #region Properties

            public TViewModel ViewModel
            {
                get { return m_viewModel; }
            }

            #endregion

            #region Events

            public event EventHandler Closed;

            internal void OnClosed(EventArgs e)
            {
                Closed.Raise(m_viewModel, e);
            }

            #endregion
        }

        #endregion
    }
}
