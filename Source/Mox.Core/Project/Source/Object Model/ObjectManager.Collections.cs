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

using Mox.Collections;

using ObjectIdentifier = System.Int32;

namespace Mox
{
    /// <summary>
    /// Manages objects... duh
    /// </summary>
    partial class ObjectManager
    {
        #region Inner Types

        private class TransactionableObjectCollection : IObjectCollection
        {
            #region Variables

            private readonly ObjectCollection m_internalCollection;

            #endregion

            #region Constructor

            public TransactionableObjectCollection(ObjectCollection internalCollection)
            {
                Debug.Assert(internalCollection != null);
                m_internalCollection = internalCollection;
            }

            #endregion

            #region Properties

            private ObjectManager Manager
            {
                get { return m_internalCollection.Manager; }
            }

            #endregion

            #region IObservableCollection<Object> Members

            public event EventHandler<CollectionChangedEventArgs<Object>> CollectionChanged
            {
                add { m_internalCollection.CollectionChanged += value; }
                remove { m_internalCollection.CollectionChanged -= value; }
            }

            #endregion

            #region ICollection<Object> Members

            public void Add(Object item)
            {
                Throw.IfNull(item, "item");
                Throw.InvalidArgumentIf(item.Manager != Manager, "Item is owned by another manager!", "item");

                Manager.Controller.Execute(new AddObjectCommand(item.Identifier));
            }

            public void Clear()
            {
                Manager.Controller.Execute(new ClearObjectsCommand());
            }

            public bool Contains(Object item)
            {
                return m_internalCollection.Contains(item);
            }

            public void CopyTo(Object[] array, int arrayIndex)
            {
                m_internalCollection.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return m_internalCollection.Count; }
            }

            public bool IsReadOnly
            {
                get { return m_internalCollection.IsReadOnly; }
            }

            public bool Remove(Object item)
            {
                if (Contains(item))
                {
                    Manager.Controller.Execute(new RemoveObjectCommand(item.Identifier));
                    return true;
                }
                return false;
            }

            #endregion

            #region IEnumerable<Object> Members

            public IEnumerator<Object> GetEnumerator()
            {
                return m_internalCollection.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        private class ObjectCollection : ObservableCollection<Object>, IObjectCollection
        {
            #region Variables

            private readonly ObjectManager m_manager;

            #endregion

            #region Inner Types

            private class ReadOnlyObjectCollection : ReadOnlyObservableCollection, IObjectCollection
            {
                #region Constructor

                public ReadOnlyObjectCollection(ObjectCollection innerCollection)
                    : base(innerCollection)
                {
                }

                #endregion

                #region Properties

                public new ObjectCollection WrappedCollection
                {
                    get { return (ObjectCollection)base.WrappedCollection; }
                }

                #endregion
            }

            #endregion

            #region Constructor

            public ObjectCollection(ObjectManager manager)
            {
                Debug.Assert(manager != null);
                m_manager = manager;
            }

            #endregion

            #region Properties

            internal ObjectManager Manager
            {
                get { return m_manager; }
            }

            #endregion

            #region Methods

            protected override IObservableCollection<Object> CreateReadOnlyWrapper()
            {
                return new ReadOnlyObjectCollection(this);
            }

            public override void Add(Object item)
            {
                Throw.InvalidArgumentIf(Contains(item), "Cannot add the same object twice", "item");
                base.Add(item);
            }

            protected override void OnCollectionChanged(CollectionChangedEventArgs<Object> e)
            {
                e.Synchronize(OnAddObject, OnRemoveObject);
                base.OnCollectionChanged(e);
            }

            private void OnAddObject(Object obj)
            {
                Manager.RegisterObject(obj);
            }

            private void OnRemoveObject(Object obj)
            {
                Manager.UnregisterObject(obj);
            }

            #endregion
        }

        #endregion
    }
}
