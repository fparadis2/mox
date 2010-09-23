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

namespace Mox.Collections
{
    /// <summary>
    /// Collection Change Action.
    /// </summary>
    public enum CollectionChangeAction
    {
        /// <summary>
        /// An item was added to the collection.
        /// </summary>
        Add,
        /// <summary>
        /// An item was removed from the collection.
        /// </summary>
        Remove,
        /// <summary>
        /// The collection is being cleared.
        /// </summary>
        Clear,
    }

    /// <summary>
    /// Collection changed event args.
    /// </summary>
    public class CollectionChangedEventArgs<T> : EventArgs
    {
        #region Variables

        private readonly CollectionChangeAction m_action;
        private readonly IList<T> m_items;

        #endregion

        #region Constructor
                
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="items"></param>
        public CollectionChangedEventArgs(CollectionChangeAction action, IList<T> items)
        {
            m_action = action;
            m_items = items;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Action that changed the collection.
        /// </summary>
        public CollectionChangeAction Action
        {
            get { return m_action; }
        }

        /// <summary>
        /// Item(s) that were added/removed from the collection.
        /// </summary>
        public IList<T> Items
        {
            get { return m_items; }
        }

        #endregion
    }
}
