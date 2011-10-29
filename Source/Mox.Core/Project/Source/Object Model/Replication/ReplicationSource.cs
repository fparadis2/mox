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
using System.Linq;
using System.Diagnostics;
using Mox.Transactions;

namespace Mox.Replication
{
    /// <summary>
    /// Listens to an object manager and forwards synchronisation events to registered replication clients.
    /// </summary>
    public class ReplicationSource<TUser> : IDisposable
    {
        #region Inner Types

        private class ViewContext
        {
            #region Variables

            private readonly IReplicationClient m_client;
            private readonly TUser m_user;

            #endregion

            #region Constructor

            public ViewContext(TUser user, IReplicationClient client)
            {
                Debug.Assert(client != null);

                m_client = client;
                m_user = user;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Client associated with this context.
            /// </summary>
            public IReplicationClient Client
            {
                get { return m_client; }
            }

            /// <summary>
            /// Key associated with this context.
            /// </summary>
            public TUser User
            {
                get { return m_user; }
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly ObjectManager m_host;
        private readonly List<ViewContext> m_viewContexts = new List<ViewContext>();
        private readonly IAccessControlStrategy<TUser> m_accessControlStrategy;
        private readonly CommandSynchronizer<TUser> m_commandSynchronizer;

        private readonly List<UserAccessChangedEventArgs<TUser>> m_pendingSynchronizations = new List<UserAccessChangedEventArgs<TUser>>();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        internal ReplicationSource(ObjectManager host, IAccessControlStrategy<TUser> accessControlStrategy)
        {
            Throw.IfNull(host, "host");
            Throw.IfNull(accessControlStrategy, "accessControlStrategy");

            m_host = host;
            m_accessControlStrategy = accessControlStrategy;

#warning TODO USER
            m_commandSynchronizer = new CommandSynchronizer<TUser>(host, m_accessControlStrategy, default(TUser));

            m_accessControlStrategy.UserAccessChanged += WhenUserAccessControlChanged;

            //TransactionStack.CommandPushed += TransactionStack_CommandPushed;
            //TransactionStack.TransactionStarted += TransactionStack_TransactionStarted;
            //TransactionStack.CurrentTransactionEnded += TransactionStack_CurrentTransactionEnded;
        }

        public void Dispose()
        {
            m_accessControlStrategy.UserAccessChanged -= WhenUserAccessControlChanged;
            m_accessControlStrategy.Dispose();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers a client.
        /// </summary>
        /// <param name="user">User to associated the client with. This will determine what the client can actually 'see'.</param>
        /// <param name="client">Client to register.</param>
        public void Register(TUser user, IReplicationClient client)
        {
            Throw.InvalidArgumentIf(m_viewContexts.Any(context => context.Client == client), "Client is already registered", "client");

            ViewContext viewContext = new ViewContext(user, client);
            m_viewContexts.Add(viewContext);
            FullySynchronize(viewContext);
        }

        #region Synchronisation

        private void Synchronize(ViewContext viewContext, ICommand commandToSynchronize)
        {
            ICommand command = m_commandSynchronizer.PrepareImmediateSynchronization(commandToSynchronize);
            if (command != null && !command.IsEmpty)
            {
                viewContext.Client.Replicate(command);
            }
        }

        private void DelayedSynchronize(ViewContext context, Object @object)
        {
#warning TODO: What happens for multiple clients?? doesn't seem to hold
            ICommand command = m_commandSynchronizer.PrepareDelayedSynchronization(@object);
            if (command != null && !command.IsEmpty)
            {
                context.Client.Replicate(command);
            }
        }

        private void FullySynchronize(ViewContext viewContext)
        {
            //IEnumerable<ICommand> undoStack = TransactionStack.UndoStack;
            //if (undoStack.Any())
            //{
            //    Synchronize(viewContext, undoStack.Reverse());
            //}
        }

        private void DoOperationOnAllContexts(Action<ViewContext> operation)
        {
            m_viewContexts.ForEach(operation);
        }

        private void DelaySynchronizeIfPossible()
        {
            foreach (UserAccessChangedEventArgs<TUser> e in m_pendingSynchronizations)
            {
                UserAccessChangedEventArgs<TUser> visibilityArgs = e;
                Debug.Assert(visibilityArgs.Access.Contains(UserAccess.Read));
                m_viewContexts.FindAll(context => Equals(context.User, visibilityArgs.User)).ForEach(context => DelayedSynchronize(context, visibilityArgs.Object));
            }
            m_pendingSynchronizations.Clear();
        }

        private void AddPendingDelayedSynchronization(UserAccessChangedEventArgs<TUser> e)
        {
            for (int i = 0; i < m_pendingSynchronizations.Count; i++)
            {
                UserAccessChangedEventArgs<TUser> existingArgs = m_pendingSynchronizations[i];
                if (existingArgs.Object == e.Object && Equals(existingArgs.User, e.User))
                {
                    if (existingArgs.Access != e.Access)
                    {
                        m_pendingSynchronizations.RemoveAt(i);
                    }
                    return;
                }
            }

            if (e.Access.Contains(UserAccess.Read))
            {
                m_pendingSynchronizations.Add(e);
            }
        }

        private 

        #endregion

        #endregion

        #region Event Handlers

        void TransactionStack_CommandPushed(object sender, CommandEventArgs e)
        {
            Debug.Assert(!e.Command.IsEmpty);

            DelaySynchronizeIfPossible();
            DoOperationOnAllContexts(context => Synchronize(context, e.Command));
        }

        void TransactionStack_TransactionStarted(object sender, TransactionStartedEventArgs e)
        {
            //DoOperationOnAllContexts(context => context.Client.BeginTransaction(e.Type));
        }

        void TransactionStack_CurrentTransactionEnded(object sender, TransactionEndedEventArgs e)
        {
            DelaySynchronizeIfPossible();

            if ((e.Type & TransactionType.Atomic) == TransactionType.Atomic)
            {
                return;
            }

            //DoOperationOnAllContexts(context => context.Client.EndCurrentTransaction(e.Rollbacked));
        }

        void WhenUserAccessControlChanged(object sender, UserAccessChangedEventArgs<TUser> e)
        {
            AddPendingDelayedSynchronization(e);
            DelaySynchronizeIfPossible();
        }

        #endregion
    }
}
