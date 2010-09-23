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
    /// Provides a base class for collections implementing <see cref="ICollection{T}"/>.
    /// </summary>
    public abstract class CollectionBase<T> : ICollection<T>
    {
        #region Properties

        /// <summary>
        /// Returns the number of items in this collection.
        /// </summary>
        public abstract int Count
        {
            get;
        }

        /// <summary>
        /// Returns whether the collection is read-only.
        /// </summary>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns true if the collection contains the given <paramref name="item"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public abstract bool Contains(T item);

        /// <summary>
        /// Adds the given <paramref name="item"/> to the collection.
        /// </summary>
        /// <param name="item"></param>
        public abstract void Add(T item);

        /// <summary>
        /// Removes the given <paramref name="item"/> from the collection.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public abstract bool Remove(T item);

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Copies the collection to the given <paramref name="array"/>.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (T item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        /// GetEnumerator.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator<T> GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
