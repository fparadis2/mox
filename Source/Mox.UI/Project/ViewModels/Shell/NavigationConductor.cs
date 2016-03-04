using System;
using System.Collections.Generic;
using Caliburn.Micro;

namespace Mox.UI
{
    public interface INavigationConductor
    {
        bool Push(object viewModel);
        bool Pop(object viewModel);
        bool TransitionTo(object viewModel);
    }

    public interface ICanClose
    {
        bool CanClose();
    }

    public class NavigationConductor : Screen, INavigationConductor
    {
        #region Variables

        private readonly Stack<object> m_viewModels = new Stack<object>();

        #endregion

        #region Properties

        public object ActiveItem
        {
            get { return m_viewModels.Count > 0 ? m_viewModels.Peek() : null; }
        }

        private void OnActiveItemChanged()
        {
            NotifyOfPropertyChange(() => ActiveItem);
        }
 
	    #endregion

        #region Methods

        public bool Push(object viewModel)
        {
            Throw.IfNull(viewModel, "viewModel");
            m_viewModels.Push(viewModel);

            IChild child = viewModel as IChild;
            if (child != null)
                child.Parent = this;

            ScreenExtensions.TryActivate(viewModel);
            OnActiveItemChanged();
            return true;
        }

        public bool Pop(object expectedViewModel)
        {
            Throw.InvalidOperationIf(!Equals(m_viewModels.Peek(), expectedViewModel), "Can only pop the top-most view model");
            
            ICanClose canClose = m_viewModels.Peek() as ICanClose;
            if (canClose != null && !canClose.CanClose())
            {
                return false;
            }

            var viewModel = m_viewModels.Pop();

            ScreenExtensions.TryDeactivate(viewModel, true);

            IChild child = viewModel as IChild;
            if (child != null)
                child.Parent = null;

            OnActiveItemChanged();
            return true;
        }

        public bool TransitionTo(object viewModel)
        {
            if (m_viewModels.Count > 0)
            {
                if (!Pop(m_viewModels.Peek()))
                    return false;
            }

            return Push(viewModel);
        }

        public override void CanClose(Action<bool> callback)
        {
            while (m_viewModels.Count > 0)
            {
                if (!Pop(m_viewModels.Peek()))
                {
                    callback(false);
                    return;
                }
            }

            base.CanClose(callback);
        }

        #endregion
    }
}
