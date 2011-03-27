using System;
using System.Collections.Generic;

namespace Mox.UI
{
    public class WorkspaceConductor<TWorkspace> : NavigationConductor<INavigationViewModel<TWorkspace>>
        where TWorkspace : IWorkspace, new()
    {
        #region Variables

        private readonly TWorkspace m_workspace = new TWorkspace();
        private readonly Stack<TWorkspace> m_stack = new Stack<TWorkspace>();

        #endregion

        #region Properties

        public TWorkspace Workspace
        {
            get { return m_workspace; }
        }

        #endregion

        #region Methods

        protected override void OnPush(INavigationViewModel<TWorkspace> viewModel)
        {
            base.OnPush(viewModel);

            var copy = new TWorkspace();
            m_workspace.AssignTo(copy);
            m_stack.Push(copy);
            viewModel.Fill(m_workspace);
        }

        protected override void OnPop()
        {
            var old = m_stack.Pop();
            old.AssignTo(m_workspace);

            base.OnPop();
        }

        #endregion
    }
}
