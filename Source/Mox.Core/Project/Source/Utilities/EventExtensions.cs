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

namespace Mox
{
    /// <summary>
    /// Event extension methods.
    /// </summary>
    public static class EventExtensions
    {
        #region Methods

        /// <summary>
        /// Raises the given <paramref name="event"/> using the specified <paramref name="sender"/> and <paramref name="eventArgs"/>.
        /// </summary>
        /// <param name="event"></param>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public static void Raise(this EventHandler @event, object sender, EventArgs eventArgs)
        {
            if (@event != null)
            {
                @event(sender, eventArgs);
            }
        }

        /// <summary>
        /// Raises the given <paramref name="event"/> using the specified <paramref name="sender"/> and <paramref name="eventArgs"/>.
        /// </summary>
        /// <param name="event"></param>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public static void Raise<TEventArgs>(this EventHandler<TEventArgs> @event, object sender, TEventArgs eventArgs)
        {
            if (@event != null)
            {
                @event(sender, eventArgs);
            }
        }

        #endregion
    }
}
