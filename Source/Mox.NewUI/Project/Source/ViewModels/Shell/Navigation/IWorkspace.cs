using System;
using System.ComponentModel;

namespace Mox.UI
{
    public interface IWorkspace : INotifyPropertyChanged
    {
        #region Methods

        void AssignTo(IWorkspace other);

        #endregion
    }
}
