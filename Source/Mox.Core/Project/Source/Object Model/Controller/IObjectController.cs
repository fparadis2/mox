using System;

namespace Mox.Transactions
{
    public interface IObjectController
    {
        /// <summary>
        /// Indicates that many changes will occur on the object during a scope.
        /// Also allows to rollback changes.
        /// </summary>
        /// <returns></returns>
        ITransaction BeginTransaction();

        /// <summary>
        /// Executes a modification on the object.
        /// </summary>
        /// <param name="command"></param>
        void Execute(ICommand command);

        /// <summary>
        /// Fired when a command is executed.
        /// </summary>
        event EventHandler<CommandEventArgs> CommandExecuted;
    }
}
