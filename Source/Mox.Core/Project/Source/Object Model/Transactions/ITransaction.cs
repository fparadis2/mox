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

namespace Mox
{
    /// <summary>
    /// Represents a transaction.
    /// </summary>
    public interface ITransaction : IDisposable
    {
        #region Properties

        /// <summary>
        /// Transaction type.
        /// </summary>
        Transactions.TransactionType Type
        {
            get;
        }

        object Token { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Rollbacks the transaction.
        /// </summary>
        void Rollback();

        /// <summary>
        /// Rollbacks the transaction but returns a transaction that will return the stack to its initial state when rolled back.
        /// </summary>
        /// <returns></returns>
        ITransaction Reverse();

        #endregion
    }
}
