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
    public class ReplicationSource<TKey> : IDisposable
    {
        #region Inner Types

        private class ViewContext
        {
            #region Variables

            private readonly IReplicationClient m_client;
            private readonly TKey m_key;

            #endregion

            #region Constructor

            public ViewContext(TKey key, IReplicationClient client)
            {
                Debug.Assert(client != null);

                m_client = client;
                m_key = key;
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
            public TKey Key
            {
                get { return m_key; }
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly ObjectManager m_host;
        private readonly List<ViewContext> m_viewContexts = new List<ViewContext>();
        private readonly IVisibilityStrategy<TKey> m_visibilityStrategy;
        private readonly ICommandSynchronizer<TKey> m_commandSynchronizer;

        private readonly List<VisibilityChangedEventArgs<TKey>> m_pendingSynchronizations = new List<VisibilityChangedEventArgs<TKey>>();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        internal ReplicationSource(ObjectManager host, IVisibilityStrategy<TKey> visibilityStrategy, ICommandSynchronizer<TKey> commandSynchronizer)
        {
            Throw.IfNull(host, "host");
            Throw.IfNull(visibilityStrategy, "visibilityStrategy");
            Throw.IfNull(commandSynchronizer, "commandSynchronizer");

            m_host = host;
            m_visibilityStrategy = visibilityStrategy;
            m_commandSynchronizer = commandSynchronizer;

            m_visibilityStrategy.ObjectVisibilityChanged += m_visibilityStrategy_ObjectVisibilityChanged;

            //TransactionStack.CommandPushed += TransactionStack_CommandPushed;
            //TransactionStack.TransactionStarted += TransactionStack_TransactionStarted;
            //TransactionStack.CurrentTransactionEnded += TransactionStack_CurrentTransactionEnded;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ReplicationSource(ObjectManager host, IVisibilityStrategy<TKey> visibilityStrategy)
            : this(host, visibilityStrategy, new CommandSynchronizer<TKey>())
        {
        }

        public void Dispose()
        {
            m_visibilityStrategy.Dispose();
        }

        #endregion

        #region Properties

        private bool MustSynchronize
        {
            get
            {
                return true;
                //return !TransactionStack.IsInAtomicTransaction;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers a client.
        /// </summary>
        /// <param name="client">Client to register.</param>
        /// <param name="key">Key to associated the listener with. This will determine what the client can actually 'see'.</param>
        public void Register(TKey key, IReplicationClient client)
        {
            Throw.InvalidArgumentIf(m_viewContexts.Any(context => context.Client == client), "Client is already registered", "client");

            ViewContext viewContext = new ViewContext(key, client);
            m_viewContexts.Add(viewContext);
            FullySynchronize(viewContext);
        }

        #region Synchronisation

        private void Synchronize(ViewContext viewContext, IEnumerable<ICommand> commandsToSynchronize)
        {
            ICommand command = m_commandSynchronizer.Synchronize(m_host, m_visibilityStrategy, viewContext.Key, commandsToSynchronize);
            if (command != null && !command.IsEmpty)
            {
                viewContext.Client.Replicate(command);
            }
        }

        private void DelayedSynchronize(ViewContext context, Object @object)
        {
            ICommand command = m_commandSynchronizer.Update(@object);
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
            if (!MustSynchronize)
            {
                return;
            }

            m_viewContexts.ForEach(operation);
        }

        private void Synchronize(ViewContext viewContext, ICommand command)
        {
            Synchronize(viewContext, new[] { command });
        }

        private void DelaySynchronizeIfPossible()
        {
            if (!MustSynchronize)
            {
                return;
            }

            foreach (VisibilityChangedEventArgs<TKey> e in m_pendingSynchronizations)
            {
                VisibilityChangedEventArgs<TKey> visibilityArgs = e;
                Debug.Assert(visibilityArgs.Visible);
                m_viewContexts.FindAll(context => Equals(context.Key, visibilityArgs.Key)).ForEach(context => DelayedSynchronize(context, visibilityArgs.Object));
            }
            m_pendingSynchronizations.Clear();
        }

        private void AddPendingDelayedSynchronization(VisibilityChangedEventArgs<TKey> e)
        {
            for (int i = 0; i < m_pendingSynchronizations.Count; i++)
            {
                VisibilityChangedEventArgs<TKey> existingArgs = m_pendingSynchronizations[i];
                if (existingArgs.Object == e.Object && Equals(existingArgs.Key, e.Key))
                {
                    if (existingArgs.Visible != e.Visible)
                    {
                        m_pendingSynchronizations.RemoveAt(i);
                    }
                    return;
                }
            }

            if (e.Visible)
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

        void m_visibilityStrategy_ObjectVisibilityChanged(object sender, VisibilityChangedEventArgs<TKey> e)
        {
            AddPendingDelayedSynchronization(e);
            DelaySynchronizeIfPossible();
        }

        #endregion
    }
}
