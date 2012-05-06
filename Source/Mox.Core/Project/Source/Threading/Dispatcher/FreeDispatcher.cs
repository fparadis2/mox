using System;

namespace Mox.Threading
{
    public class FreeDispatcher : IDispatcher
    {
        public void InvokeIfNeeded(Action action)
        {
            action();
        }

        public void BeginInvokeIfNeeded(Action action)
        {
            action();
        }
    }
}