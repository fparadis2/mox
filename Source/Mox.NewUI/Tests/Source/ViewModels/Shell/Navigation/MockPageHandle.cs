using System;

namespace Mox.UI
{
    public class MockPageHandle : IPageHandle
    {
        #region Implementation of IPageHandle

        public void OnClosed(object sender, EventArgs e)
        {
            Closed.Raise(sender, e);
        }

        public event EventHandler Closed;

        #endregion
    }
}
