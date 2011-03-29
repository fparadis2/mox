using System;

using Caliburn.Micro;

namespace Mox.UI.Shell
{
    public class MoxWorkspaceViewModel : WorkspaceConductor<MoxWorkspace>
    {
        #region Methods

        protected override object TransformWorkspaceValue(object oldValue, object newValue)
        {
            if (newValue != null)
            {
                var viewModelType = newValue.GetType();
                var viewType = ViewLocator.LocateTypeForModelType(newValue.GetType(), null, null);

                if (oldValue == null || !viewType.IsAssignableFrom(oldValue.GetType()))
                {
                    var view = ViewLocator.LocateForModelType(viewModelType, null, null);
                    ViewModelBinder.Bind(newValue, view, null);
                    return view;
                }

                return oldValue;
            }

            return base.TransformWorkspaceValue(oldValue, newValue);
        }

        #endregion
    }
}
