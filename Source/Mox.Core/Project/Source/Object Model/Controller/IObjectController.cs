using System;

namespace Mox.Transactions
{
    public interface IObjectController
    {
        #region Transactions & Groups

        bool IsInTransaction
        {
            get;
        }

        /// <summary>
        /// Allows to rollback changes made during the transaction.
        /// </summary>
        /// <returns></returns>
        void BeginTransaction(object token);
        void EndTransaction(bool rollback, object token);

        /// <summary>
        /// Indicates that many commands will be executed, all to be considered as a single command.
        /// </summary>
        /// <returns></returns>
        IDisposable BeginCommandGroup();

        #endregion

        #region Execution

        /// <summary>
        /// Executes a modification on the object.
        /// </summary>
        /// <param name="command"></param>
        void Execute(ICommand command);

        /// <summary>
        /// Fired when a command is executed.
        /// </summary>
        event EventHandler<CommandEventArgs> CommandExecuted;

        #endregion

        #region Synchronization

        /// <summary>
        /// Returns a command that synchronizes a new host to the current state of this controller's host.
        /// </summary>
        /// <returns></returns>
        ICommand CreateInitialSynchronizationCommand();

        #endregion
    }
}
