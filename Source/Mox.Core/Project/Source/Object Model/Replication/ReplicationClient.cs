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

        #endregion

        #region Constructor

        public ReplicationClient()
        {
            m_originalController = m_host.Controller;
            m_host.UpgradeController(new ReplicationController());
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
            m_originalController.Execute(command);
        }

        [Conditional("DEBUG")]
        private void EnsureHostIsReplicated()
        {
            Throw.InvalidProgramIf(!(m_host.Controller is ReplicationController), "Inconsistency in host controller");
        }

        #endregion

        #region Inner Types

        private class ReplicationController : IObjectController
        {
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
                throw new InvalidOperationException("Cannot begin command groups on a replicated host");
            }

            public void Execute(ICommand command)
            {
                throw new InvalidOperationException("Cannot execute commands on a replicated host");
            }

            public ICommand CreateInitialSynchronizationCommand()
            {
                throw new InvalidOperationException("Cannot replicate from a replication client");
            }

            public event EventHandler<CommandEventArgs> CommandExecuted
            {
                add { }
                remove { }
            }

            #endregion
        }

        #endregion
    }
}
