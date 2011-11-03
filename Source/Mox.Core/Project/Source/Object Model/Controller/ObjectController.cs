using System;
using System.Collections.Generic;

namespace Mox.Transactions
{
    public class ObjectController : IObjectController
    {
        #region Variables

        private readonly ObjectManager m_manager;
        private readonly Stack<Scope> m_scopes = new Stack<Scope>();
        private readonly MultiCommand m_pastCommands = new MultiCommand();

        #endregion

        #region Constructor

        public ObjectController(ObjectManager manager)
        {
            Throw.IfNull(manager, "manager");
            m_manager = manager;
        }

        #endregion

        #region Properties

        private Scope CurrentScope
        {
            get { return m_scopes.Count > 0 ? m_scopes.Peek() : null; }
        }

        private bool IsInGroup
        {
            get { return m_scopes.Count > 0 ? m_scopes.Peek().IsInGroup : false; }
        }

        #endregion

        #region Methods

        public void BeginTransaction(object token = null)
        {
            var transaction = new Transaction(this, token);
            m_scopes.Push(transaction);
        }

        public void EndTransaction(bool rollback, object token = null)
        {
            Throw.InvalidOperationIf(CurrentScope is Transaction == false, "EndTransaction inconsistency");
            var transaction = (Transaction)CurrentScope;
            Throw.InvalidOperationIf(!Equals(transaction.Token, token), "Inconsistent transaction token");
            transaction.End(!rollback);
        }

        public IDisposable BeginCommandGroup()
        {
            var group = new Group(this);
            m_scopes.Push(group);
            return group;
        }

        private void EndScope(Scope scope, bool commitCommand)
        {
            Throw.InvalidOperationIf(CurrentScope != scope, "Cannot dispose a scope that is not the current scope.");
            m_scopes.Pop();

            if (commitCommand)
            {
                TransferCommand(scope.Command);

                if (scope.IsInGroup && !IsInGroup)
                {
                    OnCommandExecuted(scope.Command);
                }
            }
            else
            {
                scope.Command.Unexecute(m_manager);

                if (!IsInGroup)
                {
                    OnCommandExecuted(new ReverseCommand(scope.Command));
                }
            }
        }

        public void Execute(ICommand command)
        {
            if (!command.IsEmpty)
            {
                command.Execute(m_manager);
                TransferCommand(command);

                if (!IsInGroup)
                {
                    OnCommandExecuted(command);
                }
            }
        }

        private void TransferCommand(ICommand command)
        {
            var scope = CurrentScope;
            if (scope != null)
            {
                scope.Push(command);
            }
            else
            {
                m_pastCommands.Add(command);
            }
        }

        public ICommand CreateInitialSynchronizationCommand()
        {
            return m_pastCommands;
        }

        #endregion

        #region Events

        public event EventHandler<CommandEventArgs> CommandExecuted;

        private void OnCommandExecuted(ICommand command)
        {
            CommandExecuted.Raise(this, new CommandEventArgs(command));
        }

        #endregion

        #region Inner Types

        private abstract class Scope
        {
            #region Variables

            private readonly ObjectController m_controller;
            private readonly MultiCommand m_command = new MultiCommand();
            private readonly bool m_isInGroup;
            private bool m_isDisposed;

            #endregion

            #region Constructor

            protected Scope(ObjectController controller, bool isInGroup)
            {
                Throw.IfNull(controller, "controller");
                m_controller = controller;
                m_isInGroup = isInGroup;
            }

            public void End(bool commitCommand)
            {
                if (!m_isDisposed)
                {
                    m_isDisposed = true;
                    m_controller.EndScope(this, commitCommand);
                }
            }

            #endregion

            #region Properties

            public ICommand Command
            {
                get { return m_command; }
            }

            public bool IsInGroup
            {
                get { return m_isInGroup; }
            }

            #endregion

            #region Methods

            public void Push(ICommand command)
            {
                Throw.InvalidOperationIf(m_isDisposed, "Cannot execute commands once the current transaction has been disposed or rolled back.");
                m_command.Add(command);
            }

            #endregion
        }

        private class Group : Scope, IDisposable
        {
            public Group(ObjectController controller)
                : base(controller, true)
            {
            }

            public void Dispose()
            {
                End(true);
            }
        }

        private class Transaction : Scope
        {
            #region Variables

            private readonly object m_token;

            #endregion

            #region Constructor

            public Transaction(ObjectController controller, object token)
                : base(controller, controller.IsInGroup)
            {
                m_token = token;
            }

            #endregion

            #region Properties

            public object Token
            {
                get { return m_token; }
            }

            #endregion
        }

        #endregion
    }
}
