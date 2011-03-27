using System;

using Caliburn.Micro;

namespace Mox.UI.Shell
{
    public class MoxWorkspaceViewModel : WorkspaceConductor<MoxWorkspace>
    {
        #region Methods

        protected override object TransformWorkspaceValue(object value)
        {
            if (value != null)
            {
                var view = ViewLocator.LocateForModel(value, null, null);
                ViewModelBinder.Bind(value, view, null);
                return view;
            }

            return base.TransformWorkspaceValue(value);
        }

        #endregion
    }
}
