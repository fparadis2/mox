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
using System.Diagnostics;

namespace Mox.Collections
{
    /// <summary>
    /// An observable collection.
    /// </summary>
    public partial class ObservableCollection<T> : CollectionBase<T>, IObservableCollection<T>
    {
        #region Variables

        private List<T> m_innerCollection = new List<T>();
        private IObservableCollection<T> m_readOnlyWrapper;

        #endregion

        #region Constructor

        #endregion

        #region Properties

        /// <summary>
        /// Number of items in the collection.
        /// </summary>
        public override int Count
        {
            get { return m_innerCollection.Count; }
        }

        protected IList<T> InnerCollection
        {
            get { return m_innerCollection; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns true if this collection contains the given <paramref name="item"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool Contains(T item)
        {
            return m_innerCollection.Contains(item);
        }

        /// <summary>
        /// Adds the given <paramref name="item"/> in the collection.
        /// </summary>
        /// <param name="item"></param>
        public override void Add(T item)
        {
            m_innerCollection.Add(item);
            OnCollectionChanged(new CollectionChangedEventArgs<T>(CollectionChangeAction.Add, new[] { item }));
        }

        /// <summary>
        /// Removes the given <paramref name="item"/> from the collection.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool Remove(T item)
        {
            bool result = m_innerCollection.Remove(item);

            if (result)
            {
                OnCollectionChanged(new CollectionChangedEventArgs<T>(CollectionChangeAction.Remove, new[] { item }));
            }

            return result;
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public override void Clear()
        {
            IList<T> removedItems = m_innerCollection;
            m_innerCollection = new List<T>();
            OnCollectionChanged(new CollectionChangedEventArgs<T>(CollectionChangeAction.Clear, removedItems));
        }

        /// <summary>
        /// GetEnumerator.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<T> GetEnumerator()
        {
            return m_innerCollection.GetEnumerator();
        }

        /// <summary>
        /// Returns a read-only version of this collection.
        /// </summary>
        /// <returns></returns>
        public IObservableCollection<T> AsReadOnly()
        {
            if (m_readOnlyWrapper == null)
            {
                m_readOnlyWrapper = CreateReadOnlyWrapper();
            }

            Debug.Assert(m_readOnlyWrapper != null, "Bad CreateReadOnlyWrapper implementation");
            return m_readOnlyWrapper;
        }

        protected virtual IObservableCollection<T> CreateReadOnlyWrapper()
        {
            return new ReadOnlyObservableCollection(this);
        }

        #endregion

        #region Events

        /// <summary>
        /// Triggered when the collection changes.
        /// </summary>
        public event EventHandler<CollectionChangedEventArgs<T>> CollectionChanged;

        /// <summary>
        /// Triggers the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCollectionChanged(CollectionChangedEventArgs<T> e)
        {
            CollectionChanged.Raise(this, e);
        }

        #endregion
    }
}
