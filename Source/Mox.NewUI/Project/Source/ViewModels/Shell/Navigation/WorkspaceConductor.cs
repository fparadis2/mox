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

            m_stack.Push(Clone(m_workspace));

            var workCopy = Clone(m_workspace);
            {
                viewModel.Fill(workCopy);
                Transform(workCopy);
            }
            workCopy.AssignTo(m_workspace);
        }

        protected virtual void Transform(TWorkspace workspace)
        {
        }

        protected override void OnPop()
        {
            var old = m_stack.Pop();
            old.AssignTo(m_workspace);

            base.OnPop();
        }

        private static TWorkspace Clone(TWorkspace original)
        {
            var copy = new TWorkspace();
            original.AssignTo(copy);
            return copy;
        }

        #endregion
    }
}
