namespace Mox.UI
{
    public class FreeDispatcher : IDispatcher
    {
        public void InvokeIfNeeded(System.Action action)
        {
            action();
        }

        public void BeginInvokeIfNeeded(System.Action action)
        {
            action();
        }
    }
}