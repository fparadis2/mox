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
    /// Provides a base class for read-only collections implementing <see cref="ICollection{T}"/>.
    /// </summary>
    public abstract class ReadOnlyCollectionBase<T> : CollectionBase<T>
    {
        #region Properties

        /// <summary>
        /// Returns whether the collection is read-only.
        /// </summary>
        public override bool IsReadOnly
        {
            get { return true; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the given <paramref name="item"/> to the collection.
        /// </summary>
        /// <param name="item"></param>
        public override void Add(T item)
        {
            throw ReadOnlyException();
        }

        /// <summary>
        /// Removes the given <paramref name="item"/> from the collection.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool Remove(T item)
        {
            throw ReadOnlyException();
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public override void Clear()
        {
            throw ReadOnlyException();
        }

        /// <summary>
        /// Returns the exception to be thrown when calling a method on the read-only collection.
        /// </summary>
        /// <returns></returns>
        protected NotSupportedException ReadOnlyException()
        {
            return new NotSupportedException("The collection is read-only.");
        }

        #endregion
    }

    /// <summary>
    /// Provides a base class for read-only collections implementing <see cref="ICollection{T}"/>.
    /// </summary>
    public abstract class ReadOnlyListBase<T> : ReadOnlyCollectionBase<T>, IList<T>
    {
        #region Methods

        public abstract int IndexOf(T item);

        public void Insert(int index, T item)
        {
            ReadOnlyException();
        }

        public void RemoveAt(int index)
        {
            ReadOnlyException();
        }

        public abstract T this[int index]
        {
            get; set;
        }

        #endregion
    }
}
