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
    /// Listens to a game and forwards synchronisation events to registered clients.
    /// </summary>
    public class GameViewManager : IDisposable
    {
        #region Inner Types

        private class ViewContext
        {
            #region Variables

            private readonly IGameListener m_listener;
            private readonly Player m_player;

            #endregion

            #region Constructor

            public ViewContext(IGameListener listener, Player player)
            {
                Debug.Assert(listener != null);

                m_listener = listener;
                m_player = player;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Listener associated with this context.
            /// </summary>
            public IGameListener Listener
            {
                get { return m_listener; }
            }

            /// <summary>
            /// Player associated with this context.
            /// </summary>
            public Player Player
            {
                get { return m_player; }
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly Game m_game;
        private readonly TransactionStack m_transactionStack;
        private readonly List<ViewContext> m_viewContexts = new List<ViewContext>();
        private readonly IVisibilityStrategy m_visibilityStrategy;
        private readonly ICommandSynchronizer m_commandSynchronizer;

        private readonly List<VisibilityChangedEventArgs> m_pendingSynchronizations = new List<VisibilityChangedEventArgs>();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameViewManager(Game game, TransactionStack transactionStack, IVisibilityStrategy visibilityStrategy, ICommandSynchronizer commandSynchronizer)
        {
            Throw.IfNull(game, "game");
            Throw.IfNull(transactionStack, "transactionStack");
            Throw.IfNull(visibilityStrategy, "visibilityStrategy");
            Throw.IfNull(commandSynchronizer, "commandSynchronizer");

            m_game = game;
            m_transactionStack = transactionStack;
            m_visibilityStrategy = visibilityStrategy;
            m_commandSynchronizer = commandSynchronizer;

            m_visibilityStrategy.ObjectVisibilityChanged += m_visibilityStrategy_ObjectVisibilityChanged;

            TransactionStack.CommandPushed += TransactionStack_CommandPushed;
            TransactionStack.TransactionStarted += TransactionStack_TransactionStarted;
            TransactionStack.CurrentTransactionEnded += TransactionStack_CurrentTransactionEnded;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameViewManager(Game game, IVisibilityStrategy visibilityStrategy)
            : this(game, game.TransactionStack, visibilityStrategy, new CommandSynchronizer())
        {
        }

        public void Dispose()
        {
            m_visibilityStrategy.Dispose();
        }

        #endregion

        #region Properties

        protected TransactionStack TransactionStack
        {
            get { return m_transactionStack; }
        }

        private bool MustSynchronize
        {
            get { return !TransactionStack.IsInAtomicTransaction; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers a game listener.
        /// </summary>
        /// <param name="listener">Listener to register.</param>
        /// <param name="player">Player to associated the listener with. This will determine what the listener can actually 'see'. Invalid means spectator.</param>
        public void Register(IGameListener listener, Player player)
        {
            Throw.InvalidArgumentIf(m_viewContexts.Any(context => context.Listener == listener), "Listener is already registered", "listener");
            Throw.InvalidArgumentIf(player != null && player.Manager != m_game, "Invalid player", "player");

            ViewContext viewContext = new ViewContext(listener, player);
            m_viewContexts.Add(viewContext);
            FullySynchronize(viewContext);
        }

        #region Synchronisation

        private void Synchronize(ViewContext viewContext, IEnumerable<ICommand> commandsToSynchronize)
        {
            ICommand command = m_commandSynchronizer.Synchronize(m_game, m_visibilityStrategy, viewContext.Player, commandsToSynchronize);
            if (command != null && !command.IsEmpty)
            {
                viewContext.Listener.Synchronize(command);
            }
        }

        private void DelayedSynchronize(ViewContext context, Object @object)
        {
            ICommand command = m_commandSynchronizer.Update(@object);
            if (command != null && !command.IsEmpty)
            {
                context.Listener.Synchronize(command);
            }
        }

        private void FullySynchronize(ViewContext viewContext)
        {
            IEnumerable<ICommand> undoStack = TransactionStack.UndoStack;
            if (undoStack.Any())
            {
                Synchronize(viewContext, undoStack.Reverse());
            }
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

            foreach (VisibilityChangedEventArgs e in m_pendingSynchronizations)
            {
                VisibilityChangedEventArgs visibilityArgs = e;
                Debug.Assert(visibilityArgs.Visibility);
                m_viewContexts.FindAll(context => context.Player == visibilityArgs.Player).ForEach(context => DelayedSynchronize(context, visibilityArgs.Object));
            }
            m_pendingSynchronizations.Clear();
        }

        private void AddPendingDelayedSynchronization(VisibilityChangedEventArgs e)
        {
            for (int i = 0; i < m_pendingSynchronizations.Count; i++)
            {
                VisibilityChangedEventArgs existingArgs = m_pendingSynchronizations[i];
                if (existingArgs.Object == e.Object && existingArgs.Player == e.Player)
                {
                    if (existingArgs.Visibility != e.Visibility)
                    {
                        m_pendingSynchronizations.RemoveAt(i);
                    }
                    return;
                }
            }

            if (e.Visibility)
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
            DoOperationOnAllContexts(context => context.Listener.BeginTransaction(e.Type));
        }

        void TransactionStack_CurrentTransactionEnded(object sender, TransactionEndedEventArgs e)
        {
            DelaySynchronizeIfPossible();

            if ((e.Type & TransactionType.Atomic) == TransactionType.Atomic)
            {
                return;
            }

            DoOperationOnAllContexts(context => context.Listener.EndCurrentTransaction(e.Rollbacked));
        }

        void m_visibilityStrategy_ObjectVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            AddPendingDelayedSynchronization(e);
            DelaySynchronizeIfPossible();
        }

        #endregion
    }
}
