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
using System.Diagnostics;
using Mox.Transactions;

namespace Mox.Replication
{
    /// <summary>
    /// Concrete implementation of the <see cref="IReplicationClient"/> interface.
    /// </summary>
    /// <remarks>
    /// Maintains a synchronized instance of an object manager.
    /// </remarks>
    public class ReplicationClient<T> : MarshalByRefObject, IReplicationClient
        where T : ObjectManager, new()
    {
        #region Variables

        private readonly T m_host = new T();
        private readonly IObjectController m_originalController;
        private readonly ReplicationController m_replicationController;

        #endregion

        #region Constructor

        public ReplicationClient()
        {
            m_originalController = m_host.Controller;
            m_replicationController = new ReplicationController(m_originalController);
            m_host.UpgradeController(m_replicationController);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The synchronized host.
        /// </summary>
        public T Host
        {
            get 
            {
                return m_host; 
            }
        }

        #endregion

        #region IReplicationClient Members

        public void Replicate(ICommand command)
        {
            EnsureHostIsReplicated();

            using (m_replicationController.BeginReplication())
            {
                m_originalController.Execute(command);
            }
        }

        [Conditional("DEBUG")]
        private void EnsureHostIsReplicated()
        {
            Throw.InvalidProgramIf(!(m_host.Controller is ReplicationController), "Inconsistency in host controller");
        }

        #endregion

        #region Events

        public event EventHandler<CommandEventArgs> CommandExecuted
        {
            add { m_replicationController.CommandExecuted += value; }
            remove { m_replicationController.CommandExecuted -= value; }
        }

        #endregion

        #region Inner Types

        private class ReplicationController : IObjectController
        {
            #region Variables

            private readonly IObjectController m_originalController;
            private readonly Scope m_inReplicationScope = new Scope();

            #endregion

            #region Constructor

            public ReplicationController(IObjectController originalController)
            {
                Throw.IfNull(originalController, "originalController");
                m_originalController = originalController;
                m_originalController.CommandExecuted += m_originalController_CommandExecuted;
            }

            #endregion

            #region Methods

            public IDisposable BeginReplication()
            {
                return m_inReplicationScope.Begin();
            }

            #endregion

            #region Implementation of IObjectController

            public bool IsInTransaction
            {
                get { return false; }
            }

            public void BeginTransaction(object token)
            {
                throw new InvalidOperationException("Cannot begin transactions on a replicated host");
            }

            public void EndTransaction(bool rollback, object token)
            {
                throw new InvalidOperationException("Cannot end transactions on a replicated host");
            }

            public IDisposable BeginCommandGroup()
            {
                return m_originalController.BeginCommandGroup();
            }

            public void Execute(ICommand command)
            {
                m_originalController.Execute(command);
            }

            public ICommand CreateInitialSynchronizationCommand()
            {
                throw new InvalidOperationException("Cannot replicate from a replication client");
            }

            public event EventHandler<CommandEventArgs> CommandExecuted;

            #endregion

            #region Event Handlers

            void m_originalController_CommandExecuted(object sender, CommandEventArgs e)
            {
                if (!m_inReplicationScope.InScope)
                {
                    CommandExecuted.Raise(this, e);
                }
            }

            #endregion
        }

        #endregion
    }
}
