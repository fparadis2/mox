using System;

using Mox.Transactions;

namespace Mox.AI
{
#warning TEST
    public sealed class AIObjectController : IObjectController, IDisposable
    {
        #region Variables

        private readonly AIEvaluationContext m_context;
        private IDisposable m_handle;

        private int m_numTransactions;
        private bool m_transactionRolledBack;

        #endregion

        #region Constructor

        public AIObjectController(AIEvaluationContext context)
        {
            Throw.IfNull(context, "context");

            m_context = context;
        }

        internal void SetHandle(IDisposable handle)
        {
            m_handle = handle;
        }

        public void Dispose()
        {
            DisposableHelper.SafeDispose(m_handle);
        }

        #endregion

        #region Properties

        public bool TransactionRolledback
        {
            get
            {
                return m_transactionRolledBack;
            }
        }

        public bool IsInTransaction
        {
            get { return m_numTransactions > 0; }
        }

        private IObjectController OriginalController
        {
            get { return m_context.OriginalController; }
        }

        #endregion

        #region Implementation of IObjectController

        public void BeginTransaction(object token)
        {
            m_numTransactions++;
        }

        public void EndTransaction(bool rollback, object token)
        {
            if (m_numTransactions > 0)
            {
                m_numTransactions--;
            }

            m_transactionRolledBack |= rollback;
        }

        public IDisposable BeginCommandGroup()
        {
            return null;
        }

        public void Execute(ICommand command)
        {
            OriginalController.Execute(command);
        }

        public event EventHandler<CommandEventArgs> CommandExecuted
        {
            add { throw new InvalidOperationException("Cannot synchronize from AI object controller"); }
            remove { throw new InvalidOperationException("Cannot synchronize from AI object controller"); }
        }

        public ICommand CreateInitialSynchronizationCommand()
        {
            throw new InvalidOperationException("Cannot synchronize from AI object controller");
        }

        #endregion
    }
}
