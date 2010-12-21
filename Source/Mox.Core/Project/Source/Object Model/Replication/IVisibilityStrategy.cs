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

namespace Mox.Replication
{
    /// <summary>
    /// Responsible for deciding what objects are visible to which clients.
    /// </summary>
    public interface IVisibilityStrategy<TKey> : IDisposable
    {
        #region Methods

        /// <summary>
        /// Returns true if the given <paramref name="gameObject"/> is visible for the given <paramref name="key"/>.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="key">The key to test for.</param>
        /// <returns></returns>
        bool IsVisible(Object gameObject, TKey key);

        #endregion

        #region Events

        /// <summary>
        /// Triggered when the visibility of an object changed with regards to a player.
        /// </summary>
        event EventHandler<VisibilityChangedEventArgs<TKey>> ObjectVisibilityChanged;

        #endregion
    }
}
