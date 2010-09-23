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

namespace Mox.Replication
{
    /// <summary>
    /// A visibility strategy where the whole game is open.
    /// </summary>
    public class OpenVisibilityStrategy : IVisibilityStrategy
    {
        #region Methods

        public void Dispose()
        {
        }

        /// <summary>
        /// Returns true if the given <paramref name="gameObject"/> is visible to the given <paramref name="player"/>.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="player">The player to test for. Null means a spectator.</param>
        /// <returns></returns>
        public bool IsVisible(Object gameObject, Player player)
        {
            Throw.IfNull(gameObject, "gameObject");
            return true;
        }

        #endregion

        #region Events

        /// <summary>
        /// Visibility never changes.
        /// </summary>
        public event EventHandler<VisibilityChangedEventArgs> ObjectVisibilityChanged
        {
            add { }
            remove { }
        }

        #endregion
    }
}
