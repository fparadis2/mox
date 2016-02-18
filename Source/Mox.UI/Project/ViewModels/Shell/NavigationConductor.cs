using System;
using System.Collections.Generic;
using System.Windows.Input;
using Caliburn.Micro;

namespace Mox.UI
{
    public interface INavigationConductor
    {
        void Push(object viewModel);
        void Pop(object viewModel);
    }

    public class NavigationConductor : Screen, INavigationConductor
    {
        #region Variables

        private readonly Stack<object> m_viewModels = new Stack<object>();

        #endregion

        #region Properties

        public object ActiveItem
        {
            get
            {
                return m_viewModels.Count > 0 ? m_viewModels.Peek() : null;
            }
        }

        #endregion

        #region Methods

        public void Push(object viewModel)
        {
            Throw.IfNull(viewModel, "viewModel");
            m_viewModels.Push(viewModel);

            IChild child = viewModel as IChild;
            if (child != null)
                child.Parent = this;

            IActivate activable = viewModel as IActivate;
            if (activable != null)
                activable.Activate();

            OnActiveItemChanged();
        }

        public void Pop(object expectedViewModel)
        {
            Throw.InvalidOperationIf(!Equals(m_viewModels.Peek(), expectedViewModel), "Can only pop the top-most view model");

            var viewModel = m_viewModels.Pop();

            IDeactivate deactivable = viewModel as IDeactivate;
            if (deactivable != null)
                deactivable.Deactivate(true);

            IChild child = viewModel as IChild;
            if (child != null)
                child.Parent = null;

            OnActiveItemChanged();
        }

        private void OnActiveItemChanged()
        {
            NotifyOfPropertyChange(() => ActiveItem);
        }

        #endregion
    }

    public class PageViewModel : PropertyChangedBase, IChild, IHaveDisplayName
    {
        private string m_displayName;

        public string DisplayName
        {
            get { return m_displayName; }
            set
            {
                if (m_displayName != value)
                {
                    m_displayName = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public object Parent
        {
            get { return ((IChild) this).Parent; }
        }

        object IChild.Parent { get; set; }

        protected void Close()
        {
            INavigationConductor conductor = (INavigationConductor)Parent;
            if (conductor != null)
            {
                conductor.Pop(this);
            }
        }

        public ICommand GoBackCommand
        {
            get { return new RelayCommand(Close); }
        }

        public void Show(IChild owner)
        {
            INavigationConductor conductor = owner.FindParent<INavigationConductor>();
            conductor.Push(this);
        }
    }
}
