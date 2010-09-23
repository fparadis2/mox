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
using System.Text;

namespace Mox.Transactions
{
    /// <summary>
    /// Used by AI to better control transactions during a scope.
    /// </summary>
    public interface ITransientScope
    {
        /// <summary>
        /// Whether a transaction has been rolled back during the scope.
        /// </summary>
        bool TransactionRolledback { get; }

        /// <summary>
        /// Whether there is currently a user transaction on the scope.
        /// </summary>
        bool IsInUserTransaction { get; }

        /// <summary>
        /// Uses the scope.
        /// </summary>
        /// <returns></returns>
        IDisposable Use();
    }
}
