using System;
using System.Threading;

namespace Mox.Lobby
{
    public interface IAsyncResult
    {
        /// <summary>
        /// Returns the response of the request. Blocks until the request is complete and the value is available.
        /// </summary>
        object Value { get; }

        /// <summary>
        /// Polls whether the operation has completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Checks whether the request has succeeded. Blocks until the request is complete.
        /// </summary>
        bool IsErroneous { get; }

        /// <summary>
        /// Fired when the request completes (erroneously or not).
        /// </summary>
        event System.Action Completed;
    }

    public interface IAsyncResult<out T> : IAsyncResult
    {
        /// <summary>
        /// Returns the response of the request. Blocks until the request is complete and the value is available.
        /// </summary>
        new T Value { get; }
    }
}
