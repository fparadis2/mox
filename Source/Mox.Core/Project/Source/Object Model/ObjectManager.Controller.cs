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
using System.Collections.ObjectModel;
using System.Diagnostics;
using Mox.Collections;

namespace Mox
{
    partial class ObjectManager
    {
        #region Inner Types

        /// <summary>
        /// Base class for object controllers.
        /// </summary>
        private abstract class ObjectController
        {
            #region Constructor

            protected ObjectController()
            {
            }

            #endregion

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
        private class ObjectController<T> : ObjectController
            where T : Object
        {
            #region Inner Types

            private class ControllerList : Collection<T>
            {
                #region Variables

                private readonly ObjectController<T> m_owner;

                #endregion

                #region Constructor

                public ControllerList(ObjectController<T> owner)
                {
                    Throw.IfNull(owner, "owner");
                    m_owner = owner;
                }

                #endregion

                #region Methods

                #region Overrides

                protected override void InsertItem(int index, T item)
                {
                    m_owner.Manager.Objects.Add(item);
                }

                protected override void RemoveItem(int index)
                {
                    m_owner.Manager.Objects.Remove(this[index]);
                }

                protected override void ClearItems()
                {
                    while (Count > 0)
                    {
                        m_owner.Manager.Objects.Remove(this[0]);
                    }
                }

                protected override void SetItem(int index, T item)
                {
                    throw new NotSupportedException();
                }

                #endregion

                #region Register/Unregister

                public void RegisterObject(T item)
                {
                    Debug.Assert(!Contains(item));
                    Items.Add(item);
                }

                public void UnregisterObject(T item)
                {
                    Debug.Assert(Contains(item));
                    Items.Remove(item);
                }

                #endregion

                #endregion
            }

            #endregion

            #region Variables

            private readonly ControllerList m_controlledObjects;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor.
            /// </summary>
            public ObjectController()
            {
                m_controlledObjects = new ControllerList(this);
            }

            #endregion

            #region Properties

            /// <summary>
            /// Objects controlled by this controller.
            /// </summary>
            public IList<T> ControlledObjects
            {
                get { return m_controlledObjects; }
            }

            #endregion

            #region Methods

            internal override sealed void RegisterObject(Object obj)
            {
                if (obj is T)
                {
                    CheckObjectInvariants(obj);
                    m_controlledObjects.RegisterObject((T) obj);
                }
            }

            internal override sealed void UnregisterObject(Object obj)
            {
                if (obj is T)
                {
                    CheckObjectInvariants(obj);
                    m_controlledObjects.UnregisterObject((T) obj);
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

        private readonly List<ObjectController> m_controllers = new List<ObjectController>();

        #endregion

        #region Methods

        /// <summary>
        /// Registers a controller for the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected IList<T> RegisterController<T>()
            where T : Object
        {
            ObjectController<T> controller = new ObjectController<T>
            {
                Manager = this
            };

            m_controllers.Add(controller);

            return controller.ControlledObjects;
        }

        private void RegisterObject(Object obj)
        {
            m_controllers.ForEach(controller => controller.RegisterObject(obj));
        }

        private void UnregisterObject(Object obj)
        {
            m_controllers.ForEach(controller => controller.UnregisterObject(obj));
        }

        #endregion
    }
}
