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

        #endregion

        #region Constructor

        public ReplicationClient(ReplicationControlMode mode)
        {
            Throw.InvalidArgumentIf(mode == ReplicationControlMode.Master, "Cannot have a replication client as a master", "mode");
            m_host.ChangeControlMode(ReplicationControlMode.Slave);
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

        public void Synchronize(ICommand command)
        {
            EnsureHostIsNotMaster();
            Host.TransactionStack.PushAndExecute(command);
        }

        public void BeginTransaction(TransactionType type)
        {
            EnsureHostIsNotMaster();
            Host.TransactionStack.BeginTransaction(type);
        }

        public void EndCurrentTransaction(bool rollback)
        {
            EnsureHostIsNotMaster();

            ITransaction transaction = Host.TransactionStack.CurrentTransaction;

            if (rollback)
            {
                transaction.Rollback();
            }
            else
            {
                transaction.Dispose();
            }
        }

        [Conditional("DEBUG")]
        private void EnsureHostIsNotMaster()
        {
            Throw.InvalidOperationIf(Host.ControlMode == ReplicationControlMode.Master, "Not supposed to replicate on a Master host");
        }

        #endregion
    }
}
