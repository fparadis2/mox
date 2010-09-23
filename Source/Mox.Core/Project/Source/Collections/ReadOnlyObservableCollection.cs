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
    partial class ObservableCollection<T>
    {
        /// <summary>
        /// Provides a read-only implementation of the <see cref="IObservableCollection{T}"/> interface.
        /// </summary>
        protected class ReadOnlyObservableCollection : ReadOnlyCollectionBase<T>, IObservableCollection<T>
        {
            #region Variables

            private readonly IObservableCollection<T> m_wrappedCollection;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructs a read-only wrapper over the given <paramref name="collection"/>.
            /// </summary>
            /// <remarks>
            /// Attaches strongly on the wrapped collection so is meant to be kept alive for the duration of the wrapped collection's lifetime.
            /// </remarks>
            /// <param name="collection"></param>
            public ReadOnlyObservableCollection(IObservableCollection<T> collection)
            {
                Throw.IfNull(collection, "collection");
                m_wrappedCollection = collection;
                m_wrappedCollection.CollectionChanged += m_wrappedCollection_CollectionChanged;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Number of items in the collection.
            /// </summary>
            public override int Count
            {
                get { return m_wrappedCollection.Count; }
            }

            /// <summary>
            /// The collection we are wrapping.
            /// </summary>
            protected IObservableCollection<T> WrappedCollection
            {
                get { return m_wrappedCollection; }
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
                return m_wrappedCollection.Contains(item);
            }

            /// <summary>
            /// GetEnumerator.
            /// </summary>
            /// <returns></returns>
            public override IEnumerator<T> GetEnumerator()
            {
                return m_wrappedCollection.GetEnumerator();
            }

            #endregion

            #region Event Handlers

            void m_wrappedCollection_CollectionChanged(object sender, CollectionChangedEventArgs<T> e)
            {
                OnCollectionChanged(e);
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
            private void OnCollectionChanged(CollectionChangedEventArgs<T> e)
            {
                CollectionChanged.Raise(this, e);
            }

            #endregion
        }
    }
}
