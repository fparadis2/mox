using System;
using System.ComponentModel;

namespace Mox.UI
{
    public interface IWorkspaceView : INotifyPropertyChanged
    {
        #region Methods

        void AssignTo(IWorkspaceView other);

        #endregion
    }
}
