using System;

namespace Mox.UI
{
    public interface IDispatcher
    {
        #region Methods

        void InvokeIfNeeded(System.Action action);
        void BeginInvokeIfNeeded(System.Action action);

        #endregion
    }
}
