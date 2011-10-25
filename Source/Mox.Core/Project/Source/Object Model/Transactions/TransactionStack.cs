// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mox.Transactions
{
    /// <summary>
    /// Manages transactions.
    /// </summary>
    public class TransactionStack : IDisposable, IObjectController
    {
        #region Inner Types

        private class TransactionBase : ITransaction
        {
            #region Variables

            private readonly TransactionStack m_owner;
            private readonly TransactionType m_type;
            private readonly object m_token;
            private bool m_disposed;

            #endregion

            #region Constructor

            protected TransactionBase(TransactionStack owner, TransactionType type, object token)
            {
                Debug.Assert(owner != null);
                m_owner = owner;
                m_type = type;
                m_token = token;

                if (Owner.m_currentScope != null && !IsMaster)
                {
                    Owner.m_currentScope.Begin();
                }
            }

            public void Dispose()
            {
                if (!m_disposed)
                {
                    End(false);
                }
            }

            protected virtual bool End(bool rollback)
            {
                if (Owner.m_currentScope != null && !IsMaster)
                {
                    Owner.m_currentScope.EndActiveUserTransaction(rollback);
                    return false;
                }

                m_disposed = true;
                return true;
            }

            #endregion

            #region Properties

            protected bool IsDisposed
            {
                get { return m_disposed; }
            }

            protected TransactionStack Owner
            {
                get { return m_owner; }
            }

            public object Token
            {
                get { return m_token; }
            }

            /// <summary>
            /// Type of transaction.
            /// </summary>
            public bool IsAtomic
            {
                get { return (m_type & TransactionType.Atomic) == TransactionType.Atomic; }
            }

            internal bool IsMaster
            {
                get { return (m_type & TransactionType.Master) == TransactionType.Master; }
            }

            public TransactionType Type
            {
                get { return m_type; }
            }

            #endregion

            #region Methods

            /// <summary>
            /// Rollbacks the transaction.
            /// </summary>
            public void Rollback()
            {
                Throw.DisposedIf(m_disposed, "Transaction");
                using (m_owner.BeginIsRollbacking())
                {
                    End(true);
                }
            }

            public virtual ITransaction Reverse()
            {
                return m_owner.BeginTransaction(Type);
            }

            #endregion
        }

        private class Transaction : TransactionBase
        {
            #region Variables

            private readonly MultiCommand m_multiCommand = new MultiCommand();

            #endregion

            #region Constructor

            public Transaction(TransactionStack owner, TransactionType type, object token)
                : base(owner, type, token)
            {
                Owner.m_transactions.Push(this);
            }

            #endregion

            #region Methods

            public void Push(ICommand command)
            {
                Debug.Assert(command != null);
                Debug.Assert(!IsDisposed);

                m_multiCommand.Push(command);
            }

            protected override bool End(bool rollback)
            {
                if (!base.End(rollback))
                {
                    return false;
                }

                ThrowIfNotCurrentTransaction(rollback ? "rollback" : "commit");

                if (rollback)
                {
                    m_multiCommand.Unexecute(Owner.m_owner);
                    m_multiCommand.Dispose();
                }

                Owner.m_transactions.Pop();

                // Transfer ownership of commands to parent transaction
                if (m_multiCommand.CommandCount > 0 && (Type & TransactionType.DisableStack) != TransactionType.DisableStack)
                {
                    Owner.PushImpl(m_multiCommand, IsAtomic && !Owner.IsInAtomicTransaction);
                }

                Owner.OnCurrentTransactionEnded(new TransactionEndedEventArgs(Type, rollback));
                return true;
            }

            public override ITransaction Reverse()
            {
                ITransaction transaction = base.Reverse();

                Owner.PushAndExecute(new ReverseCommand(m_multiCommand));

                return transaction;
            }

            [Conditional("DEBUG")]
            private void ThrowIfNotCurrentTransaction(string operation)
            {
                Throw.InvalidOperationIf(Owner.CurrentTransactionInternal != this, string.Format("Cannot {0} a transaction that is not the current transaction", operation));
            }

            #endregion
        }

        private class TransientTransaction : TransactionBase
        {
            public TransientTransaction(TransactionStack owner, TransactionType type, object token)
                : base(owner, type, token)
            {
            }
        }

        private class TransientScope : ITransientScope
        {
            #region Variables

            private readonly TransactionStack m_stack;
            private int m_numUserTransactions;

            #endregion

            #region Constructor

            public TransientScope(TransactionStack stack)
            {
                Throw.IfNull(stack, "stack");
                Throw.InvalidOperationIf(stack.m_currentScope != null, "Cannot create a transient scope while one is already active");
                m_stack = stack;
            }

            #endregion

            #region Properties

            public bool TransactionRolledback
            {
                get;
                internal set;
            }

            public bool IsInUserTransaction
            {
                get { return m_numUserTransactions > 0; }
            }

            #endregion

            #region Methods

            internal void Begin()
            {
                m_numUserTransactions++;
            }

            internal void EndActiveUserTransaction(bool rollback)
            {
                if (m_numUserTransactions > 0)
                {
                    m_numUserTransactions--;
                }

                TransactionRolledback |= rollback;
            }

            public IDisposable Use()
            {
                TransientScope oldScope = m_stack.m_currentScope;
                m_stack.m_currentScope = this;

                Debug.Assert(oldScope == null || oldScope == this, "Cannot have two transient scopes playing with the same stack at the same time!");

                bool oldRolledBack = TransactionRolledback;
                int oldNumUserTransactions = m_numUserTransactions;

                return new DisposableHelper(() =>
                {
                    TransactionRolledback = oldRolledBack;
                    m_numUserTransactions = oldNumUserTransactions;

                    m_stack.m_currentScope = oldScope;
                });
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly ObjectManager m_owner;
        private readonly Stack<Transaction> m_transactions = new Stack<Transaction>();
        private readonly Stack<ICommand> m_undoStack = new Stack<ICommand>();

        private TransientScope m_currentScope;
        private bool m_isRollbacking;

        #endregion

        #region Constructor

        public TransactionStack(ObjectManager owner)
        {
            Throw.IfNull(owner, "owner");
            m_owner = owner;
        }

        /// <summary>
        /// Disposes the stack.
        /// </summary>
        public void Dispose()
        {
            Throw.InvalidOperationIf(IsInTransaction, "Cannot dispose a transaction stack while there is an open transaction.");
            m_undoStack.Dispose();
            m_undoStack.Clear();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Undo Stack.
        /// </summary>
        public IEnumerable<ICommand> UndoStack
        {
            get { return m_undoStack; }
        }

        /// <summary>
        /// Returns true if there is an open transaction.
        /// </summary>
        public bool IsInTransaction
        {
            get { return m_transactions.Count > 0; }
        }

        /// <summary>
        /// Current Transaction, if any.
        /// </summary>
        public ITransaction CurrentTransaction
        {
            get { return CurrentTransactionInternal; }
        }

        private Transaction CurrentTransactionInternal
        {
            get { return IsInTransaction ? m_transactions.Peek() : null; }
        }

        public bool IsInMasterTransaction
        {
            get
            {
                return m_transactions.Any(t => t.IsMaster);
            }
        }

        public bool IsInAtomicTransaction
        {
            get
            {
                return m_transactions.Any(t => t.IsAtomic);
            }
        }

        /// <summary>
        /// Returns whether the stack is currently rollbacking.
        /// </summary>
        public bool IsRollbacking
        {
            get
            {
                return m_isRollbacking;
            }
        }

        #endregion

        #region Methods

        #region Strategy

        public ITransientScope CreateTransientScope()
        {
            return new TransientScope(this);
        }

        #endregion

        #region Transactions / Push

        /// <summary>
        /// Begins a transaction.
        /// </summary>
        /// <returns></returns>
        public ITransaction BeginTransaction()
        {
            return BeginTransaction(TransactionType.Atomic);
        }

        /// <summary>
        /// Begins a transaction.
        /// </summary>
        /// <returns></returns>
        public ITransaction BeginTransaction(TransactionType type)
        {
            return BeginTransaction(type, null);
        }

        public ITransaction BeginTransaction(TransactionType type, object token)
        {
            ITransaction transaction = CreateTransaction(type, token);
            OnTransactionStarted(new TransactionStartedEventArgs(type));
            return transaction;
        }

        private ITransaction CreateTransaction(TransactionType type, object token)
        {
            if (m_currentScope != null && (type & TransactionType.Master) == TransactionType.None)
            {
                return new TransientTransaction(this, type, token);
            }

            return new Transaction(this, type, token);
        }

        public void EndActiveUserTransaction(bool rollback, object token)
        {
            if (m_currentScope != null)
            {
                m_currentScope.EndActiveUserTransaction(rollback);
            }
            else
            {
                Throw.InvalidOperationIf(CurrentTransaction == null, "There is no current transaction");
                Throw.InvalidOperationIf((CurrentTransactionInternal.Type & TransactionType.Master) == TransactionType.Master, "Current transaction is not a user transaction.");
                Throw.InvalidOperationIf(CurrentTransactionInternal.Token != token, "Incoherent token");

                if (rollback)
                {
                    CurrentTransaction.Rollback();
                }
                else
                {
                    CurrentTransaction.Dispose();
                }
            }
        }

        /// <summary>
        /// Pushes the given <paramref name="command"/> on the stack.
        /// </summary>
        /// <remarks>
        /// The stack takes 'ownership' of the <paramref name="command"/>.
        /// </remarks>
        /// <param name="command"></param>
        public void Push(ICommand command)
        {
            PushWithAction(command, null);
        }

        /// <summary>
        /// Pushes and executes the given <paramref name="command"/> on the stack.
        /// </summary>
        /// <remarks>
        /// The stack takes 'ownership' of the <paramref name="command"/>.
        /// </remarks>
        /// <param name="command"></param>
        public void PushAndExecute(ICommand command)
        {
            PushWithAction(command, () => command.Execute(m_owner));
        }

        private void PushWithAction(ICommand command, Action action)
        {
            Throw.IfNull(command, "command");

            if (command.IsEmpty)
            {
                command.Dispose();
                return;
            }

            if (action != null)
            {
                action();
            }

            PushImpl(command, !IsInAtomicTransaction);
        }

        private void PushImpl(ICommand command, bool triggerEvent)
        {
            Debug.Assert(command != null);
            Debug.Assert(!IsRollbacking, "Cannot push commands while rollbacking");

            Transaction currentTransaction = CurrentTransactionInternal;
            if (currentTransaction != null)
            {
                currentTransaction.Push(command);
            }
            else
            {
                m_undoStack.Push(command);
            }

            if (triggerEvent)
            {
                OnCommandPushed(new CommandEventArgs(command));
            }
        }

        private IDisposable BeginIsRollbacking()
        {
            Debug.Assert(!m_isRollbacking, "Already rollbacking!");
            m_isRollbacking = true;

            return new DisposableHelper(() => m_isRollbacking = false);
        }

        internal void ClearUndoStack()
        {
            // For tests
            m_undoStack.Clear();
        }

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Triggered when a command is pushed
        /// </summary>
        public event EventHandler<CommandEventArgs> CommandPushed;

        /// <summary>
        /// Triggers the CommandPushed event.
        /// </summary>
        protected void OnCommandPushed(CommandEventArgs e)
        {
            CommandPushed.Raise(this, e);
        }

        /// <summary>
        /// Triggered when a transaction is started
        /// </summary>
        public event EventHandler<TransactionStartedEventArgs> TransactionStarted;

        /// <summary>
        /// Triggers the TransactionStarted event.
        /// </summary>
        protected void OnTransactionStarted(TransactionStartedEventArgs e)
        {
            TransactionStarted.Raise(this, e);
        }

        /// <summary>
        /// Triggered when the current transactions ends
        /// </summary>
        public event EventHandler<TransactionEndedEventArgs> CurrentTransactionEnded;

        /// <summary>
        /// Triggers the CurrentTransactionEnded event.
        /// </summary>
        protected void OnCurrentTransactionEnded(TransactionEndedEventArgs e)
        {
            CurrentTransactionEnded.Raise(this, e);
        }

        #endregion

        #region Implementation of IObjectController

        ITransaction IObjectController.BeginTransaction()
        {
            return BeginTransaction();
        }

        void IObjectController.Execute(ICommand command)
        {
            PushAndExecute(command);
        }

        #endregion
    }
}
