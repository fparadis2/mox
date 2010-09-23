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

namespace Mox.Transactions
{
    /// <summary>
    /// Types of transactions.
    /// </summary>
    [Flags]
    public enum TransactionType
    {
        /// <summary>
        /// Transaction is non-atomic.
        /// </summary>
        None = 0,
        /// <summary>
        /// Transaction is atomic. It is replicated as one command.
        /// </summary>
        Atomic = 1,
        /// <summary>
        /// Commands are not pushed during the transaction. Use with caution!
        /// </summary>
        DisableStack  = 2,
        /// <summary>
        /// Master transactions are not affected by transient scopes.
        /// </summary>
        Master = 4
    }
}