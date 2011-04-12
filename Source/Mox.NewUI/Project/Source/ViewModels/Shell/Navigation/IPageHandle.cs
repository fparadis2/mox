using System;

namespace Mox.UI
{
    public interface IPageHandle
    {
        /// <summary>
        /// Triggered when the page is closed/popped.
        /// </summary>
        event EventHandler Closed;
    }
}