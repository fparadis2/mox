using System;

namespace Mox.Threading
{
    public interface IDispatcher
    {
        #region Methods

        void InvokeIfNeeded(Action action);
        void BeginInvokeIfNeeded(Action action);

        #endregion
    }
}
