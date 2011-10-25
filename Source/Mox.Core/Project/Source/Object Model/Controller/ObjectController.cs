using System;
using System.Collections.Generic;

namespace Mox.Transactions
{
    public class ObjectController : IObjectController
    {
        #region Variables

        private readonly ObjectManager m_manager;
        private readonly Stack<Transaction> m_transactions = new Stack<Transaction>();

        #endregion

        #region Constructor

        public ObjectController(ObjectManager manager)
        {
            Throw.IfNull(manager, "manager");
            m_manager = manager;
        }

        #endregion

        #region Properties

        private Transaction CurrentTransaction
        {
            get { return m_transactions.Count > 0 ? m_transactions.Peek() : null; }
        }

        #endregion

        #region Methods

        public ITransaction BeginTransaction()
        {
            var transaction = new Transaction(this);
            m_transactions.Push(transaction);
            return transaction;
        }

        private void EndTransaction(Transaction transaction)
        {
            Throw.InvalidOperationIf(CurrentTransaction != transaction, "Cannot dispose a transaction that is not the current transaction.");
            m_transactions.Pop();

            PushCommand(transaction.Command);
        }

        public void Execute(ICommand command)
        {
            if (!command.IsEmpty)
            {
                PushCommand(command);
                command.Execute(m_manager);
            }
        }

        private void PushCommand(ICommand command)
        {
            var transaction = CurrentTransaction;
            if (transaction != null)
            {
                transaction.Push(command);
            }
        }

        #endregion

        #region Inner Types

        private class Transaction : ITransaction
        {
            #region Variables

            private readonly ObjectController m_controller;
            private readonly MultiCommand m_command = new MultiCommand();
            private bool m_rolledBack;

            #endregion

            #region Constructor

            public Transaction(ObjectController controller)
            {
                Throw.IfNull(controller, "controller");
                m_controller = controller;
            }

            public void Dispose()
            {
                m_controller.EndTransaction(this);
            }

            #endregion

            #region Properties

            public ICommand Command
            {
                get { return m_command; }
            }

            #endregion

            #region Methods

            public void Push(ICommand command)
            {
                Throw.InvalidOperationIf(m_rolledBack, "Cannot execute commands once the current transaction has been rolled back.");
                m_command.Push(command);
            }

            public void Rollback()
            {
                if (!m_rolledBack)
                {
                    m_rolledBack = true;
                    m_command.Unexecute(m_controller.m_manager);
                }
            }

            #endregion
        }

        #endregion
    }
}
