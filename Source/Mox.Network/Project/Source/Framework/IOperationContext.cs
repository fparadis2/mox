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

namespace Mox.Network
{
    /// <summary>
    /// Allows unit-testing of WCF services.
    /// </summary>
    public interface IOperationContext
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="System.String"/> used to identify the current session.
        /// </summary>
        string SessionId { get; }

        /// <summary>
        /// Get the dns of the local host.
        /// </summary>
        string LocalHostDns { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a channel to the client instance that called the current operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetCallbackChannel<T>();

        #endregion
    }
}
