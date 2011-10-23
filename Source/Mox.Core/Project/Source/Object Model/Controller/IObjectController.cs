using System;

namespace Mox.Transactions
{
    public interface IObjectController
    {
        /// <summary>
        /// Indicates that many changes will occur on the object during a scope.
        /// </summary>
        /// <returns></returns>
        IDisposable BeginTransaction();

        /// <summary>
        /// Executes a modification on the object.
        /// </summary>
        /// <param name="command"></param>
        void Execute(ICommand command);
    }
}
