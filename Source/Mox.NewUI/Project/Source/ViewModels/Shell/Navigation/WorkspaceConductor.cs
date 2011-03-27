using System;
using System.Collections.Generic;

namespace Mox.UI
{
    public interface INavigationViewModel<in TWorkspaceView>
    {
        void Fill(TWorkspaceView view);
    }

    public class WorkspaceConductor<TWorkspaceView> : NavigationConductor<INavigationViewModel<TWorkspaceView>>
        where TWorkspaceView : IWorkspaceView, new()
    {
        #region Variables

        private readonly TWorkspaceView m_view = new TWorkspaceView();
        private readonly Stack<TWorkspaceView> m_stack = new Stack<TWorkspaceView>();

        #endregion

        #region Properties

        public TWorkspaceView View
        {
            get { return m_view; }
        }

        #endregion

        #region Methods

        protected override void OnPush(INavigationViewModel<TWorkspaceView> viewModel)
        {
            base.OnPush(viewModel);

            var copy = new TWorkspaceView();
            m_view.AssignTo(copy);
            m_stack.Push(copy);
            viewModel.Fill(m_view);
        }

        protected override void OnPop()
        {
            var old = m_stack.Pop();
            old.AssignTo(m_view);

            base.OnPop();
        }

        #endregion
    }
}
