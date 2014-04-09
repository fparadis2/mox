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
using Mox.Transactions;

using ObjectIdentifier = System.Int32;

namespace Mox
{
    /// <summary>
    /// Manages objects... duh
    /// </summary>
    partial class ObjectManager
    {
        [Serializable]
        private class CreateObjectCommand<T> : Command
            where T : Object, new()
        {
            #region Variables

            private readonly Type m_scopeType;

            #endregion

            #region Constructor

            public CreateObjectCommand(Type scopeType)
            {
                m_scopeType = scopeType;
            }

            #endregion

            #region Properties

            public ObjectIdentifier ObjectIdentifier
            {
                get;
                private set;
            }

            #endregion

            #region Methods

            public override void Execute(ObjectManager manager)
            {
                ObjectIdentifier = manager.m_nextIdentifier;

                manager.CreateImpl<T>(ObjectIdentifier, m_scopeType);
                manager.m_nextIdentifier = ObjectIdentifier + 1;
            }

            public override void Unexecute(ObjectManager manager)
            {
                manager.m_nextIdentifier = ObjectIdentifier;
            }

            #endregion
        }

        /// <summary>
        /// Command that adds an item to the manager objects.
        /// </summary>
        [Serializable]
        private class AddObjectCommand : Object.ObjectCommand
        {
            #region Variables

            [NonSerialized]
            private Object m_addedObject;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor.
            /// </summary>
            public AddObjectCommand(ObjectIdentifier identifier)
                : base(identifier)
            {
            }

            #endregion

            #region Methods

            /// <summary>
            /// Adds the item to the collection.
            /// </summary>
            public override void Execute(ObjectManager manager)
            {
                if (m_addedObject != null)
                {
                    manager.AddObjectToMap(m_addedObject);
                }

                manager.AddObject(ObjectIdentifier);
            }

            /// <summary>
            /// Removes the item from the collection.
            /// </summary>
            public override void Unexecute(ObjectManager manager)
            {
                m_addedObject = manager.RemoveObject(ObjectIdentifier);
                manager.RemoveObjectFromMap(ObjectIdentifier);
            }

            #endregion
        }

        /// <summary>
        /// Command that removes an object from the manager objects.
        /// </summary>
        [Serializable]
        private class RemoveObjectCommand : Object.ObjectCommand
        {
            #region Variables

            [NonSerialized]
            private Object m_removedObject;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor.
            /// </summary>
            public RemoveObjectCommand(ObjectIdentifier identifier)
                : base(identifier)
            {
            }

            #endregion

            #region Methods

            /// <summary>
            /// Adds the item to the collection.
            /// </summary>
            public override void Execute(ObjectManager manager)
            {
                m_removedObject = manager.RemoveObject(ObjectIdentifier);
                manager.RemoveObjectFromMap(ObjectIdentifier);
            }

            /// <summary>
            /// Removes the item from the collection.
            /// </summary>
            public override void Unexecute(ObjectManager manager)
            {
                manager.AddObjectToMap(m_removedObject);
                manager.AddObject(ObjectIdentifier);
            }

            #endregion
        }

        /// <summary>
        /// Command that clears all objects from the manager.
        /// </summary>
        [Serializable]
        private class ClearObjectsCommand : Command
        {
            #region Variables

            private readonly List<ObjectIdentifier> m_removedObjects = new List<ObjectIdentifier>();

            #endregion

            #region Methods

            /// <summary>
            /// Adds the item to the collection.
            /// </summary>
            public override void Execute(ObjectManager manager)
            {
                m_removedObjects.Clear();
                manager.Objects.ForEach(o => m_removedObjects.Add(o.Identifier));
                manager.ClearObjects();
            }

            public override void Unexecute(ObjectManager manager)
            {
                m_removedObjects.ForEach(manager.AddObject);
            }

            #endregion
        }
    }
}
