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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Mox
{
    partial class ObjectManager
    {
        #region Inner Types

        /// <summary>
        /// Base class for object controllers.
        /// </summary>
        private abstract class Inventory
        {
            #region Properties

            internal ObjectManager Manager
            {
                get;
                set;
            }

            #endregion

            #region Methods

            internal virtual void RegisterObject(Object obj)
            {
            }

            internal virtual void UnregisterObject(Object obj)
            {
            }

            #endregion
        }

        /// <summary>
        /// Controls a certain type of objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class Inventory<T> : Inventory
            where T : Object
        {
            #region Inner Types

            private class ControllerList : IList<T>
            {
                #region Variables

                private readonly Inventory<T> m_owner;
                private readonly List<T> m_internalList = new List<T>(); 

                #endregion

                #region Constructor

                public ControllerList(Inventory<T> owner)
                {
                    Throw.IfNull(owner, "owner");
                    m_owner = owner;
                }

                #endregion

                #region Methods

                #region Register/Unregister

                public void RegisterObject(T item)
                {
                    int index = IndexOf(item);
                    Debug.Assert(index < 0);
                    m_internalList.Insert(~index, item);
                }

                public void UnregisterObject(T item)
                {
                    int index = IndexOf(item);
                    Debug.Assert(index >= 0);
                    m_internalList.RemoveAt(index);
                }

                #endregion

                #endregion

                #region IList<T>

                public IEnumerator<T> GetEnumerator()
                {
                    return m_internalList.GetEnumerator();
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }

                public void Add(T item)
                {
                    m_owner.Manager.Objects.Add(item);
                }

                public void Clear()
                {
                    while (Count > 0)
                    {
                        m_owner.Manager.Objects.Remove(this[Count - 1]);
                    }
                }

                public void CopyTo(T[] array, int arrayIndex)
                {
                    m_internalList.CopyTo(array, arrayIndex);
                }

                public bool Remove(T item)
                {
                    return m_owner.Manager.Objects.Remove(item);
                }

                public int Count { get { return m_internalList.Count; } }

                public bool IsReadOnly { get { return false; } }

                public bool Contains(T item)
                {
                    return IndexOf(item) >= 0;
                }

                public int IndexOf(T item)
                {
                    return m_internalList.BinarySearch(item, IdentifierComparer.Instance);
                }

                public void Insert(int index, T item)
                {
                    throw new NotSupportedException("Cannot insert in an inventory");
                }

                public void RemoveAt(int index)
                {
                    m_owner.Manager.Objects.Remove(this[index]);
                }

                public T this[int index]
                {
                    get { return m_internalList[index]; }
                    set { throw new NotSupportedException("Cannot replace an item in an inventory"); }
                }

                #endregion

                #region Inner Types

                private class IdentifierComparer : IComparer<T>
                {
                    public static readonly IdentifierComparer Instance = new IdentifierComparer();

                    public int Compare(T x, T y)
                    {
                        return x.Identifier.CompareTo(y.Identifier);
                    }
                }

                #endregion
            }

            #endregion

            #region Variables

            private readonly ControllerList m_objects;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor.
            /// </summary>
            public Inventory()
            {
                m_objects = new ControllerList(this);
            }

            #endregion

            #region Properties

            /// <summary>
            /// Objects controlled by this controller.
            /// </summary>
            public IList<T> Objects
            {
                get { return m_objects; }
            }

            #endregion

            #region Methods

            internal override sealed void RegisterObject(Object obj)
            {
                if (obj is T)
                {
                    CheckObjectInvariants(obj);
                    m_objects.RegisterObject((T) obj);
                }
            }

            internal override sealed void UnregisterObject(Object obj)
            {
                if (obj is T)
                {
                    CheckObjectInvariants(obj);
                    m_objects.UnregisterObject((T) obj);
                }
            }

            [Conditional("DEBUG")]
            private void CheckObjectInvariants(Object obj)
            {
                Debug.Assert(obj != null);
                Debug.Assert(obj is T, "Inconsistent object CLR type!");
                Debug.Assert(obj.Manager == Manager, "Inconsistent object manager!");
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly List<Inventory> m_inventories = new List<Inventory>();

        #endregion

        #region Methods

        /// <summary>
        /// Registers a controller for the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected IList<T> RegisterInventory<T>()
            where T : Object
        {
            Inventory<T> controller = new Inventory<T>
            {
                Manager = this
            };

            m_inventories.Add(controller);

            return controller.Objects;
        }

        private void RegisterObject(Object obj)
        {
            m_inventories.ForEach(controller => controller.RegisterObject(obj));
        }

        private void UnregisterObject(Object obj)
        {
            m_inventories.ForEach(controller => controller.UnregisterObject(obj));
        }

        #endregion
    }
}
