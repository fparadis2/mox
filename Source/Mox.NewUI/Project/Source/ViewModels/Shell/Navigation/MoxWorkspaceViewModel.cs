using System;

using Caliburn.Micro;

namespace Mox.UI.Shell
{
    public class MoxWorkspaceViewModel : WorkspaceConductor<MoxWorkspace>
    {
        #region Methods

        protected override void Transform(MoxWorkspace workspace)
        {
            base.Transform(workspace);

            workspace.LeftView = ViewModelToView(workspace.LeftView);
            workspace.CenterView = ViewModelToView(workspace.CenterView);
            workspace.RightView = ViewModelToView(workspace.RightView);
            workspace.BottomView = ViewModelToView(workspace.BottomView);
            workspace.CommandView = ViewModelToView(workspace.CommandView);
        }

        protected virtual object ViewModelToView(object viewModel)
        {
            if (viewModel != null)
            {
                var view = ViewLocator.LocateForModel(viewModel, null, null);
                ViewModelBinder.Bind(viewModel, view, null);
                return view;
            }

            return viewModel;
        }

        #endregion
    }
}
