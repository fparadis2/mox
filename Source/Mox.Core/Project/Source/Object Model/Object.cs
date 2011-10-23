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

using PropertyKey = System.String;
using ObjectIdentifier = System.Int32;

namespace Mox
{
    /// <summary>
    /// Base object, allows easy serialization, transfer, transactions and replication of data.
    /// </summary>
    public abstract partial class Object : IObject
    {
        #region Variables

        public static readonly Property<Type> ScopeTypeProperty = Property<Type>.RegisterProperty("ScopeType", typeof(Object), PropertyFlags.ReadOnly);

        private ObjectManager m_manager;
        private ObjectIdentifier m_identifier;
        private IObjectScope m_scope;

        private List<PropertyChangedEventArgs> m_propertyChangedQueue;

        #endregion

        #region Properties

        public ObjectManager Manager
        {
            get { return m_manager; }
        }

        /// <summary>
        /// Transaction Stack.
        /// </summary>
        protected IObjectController ObjectController
        {
            get { return Manager.Controller; }
        }

        /// <summary>
        /// Identifier for this object.
        /// </summary>
        public ObjectIdentifier Identifier
        {
            get { return m_identifier; }
        }

        public IObjectScope Scope
        {
            get
            {
                return m_scope;
            }
        }

        private Type ScopeType
        {
            get
            {
                return GetValue(ScopeTypeProperty);
            }
        }

        #endregion

        #region Methods

        #region Initialization

        protected internal virtual void Init()
        {
            CreateScope();
        }

        private void CreateScope()
        {
            Debug.Assert(m_scope == null);

            Type scopeType = ScopeType;
            if (scopeType != null)
            {
                Debug.Assert(typeof (IObjectScope).IsAssignableFrom(scopeType));
                m_scope = (IObjectScope)Activator.CreateInstance(scopeType);
                m_scope.Init(this);
            }
        }

        protected internal virtual void Uninit()
        {
            DestroyScope();
        }

        private void DestroyScope()
        {
            if (m_scope != null)
            {
                m_scope.Uninit(this);
                m_scope = null;
            }
        }

        #endregion

        #region Events

        internal IDisposable SuspendPropertyChangedEvents()
        {
            Debug.Assert(m_propertyChangedQueue == null, "Already suspended. Re-entrant call?");
            m_propertyChangedQueue = new List<PropertyChangedEventArgs>();

            return new DisposableHelper(() =>
            {
                var propertyChangedQueue = m_propertyChangedQueue;
                m_propertyChangedQueue = null;

                foreach (var e in propertyChangedQueue)
                {
                    OnPropertyChanged(e);
                }
            });
        }

        /// <summary>
        /// Triggered when the value of a property on this object changes.
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs> PropertyChanged;
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (m_propertyChangedQueue != null)
            {
                m_propertyChangedQueue.Add(e);
                return;
            }

            Manager.OnPropertyChanged(e);
            PropertyChanged.Raise(this, e);
        }

        /// <summary>
        /// Triggered when the value of a property is about to be changed on this object.
        /// </summary>
        /// <remarks>
        /// Triggered before effects layer (before final value is known).
        /// </remarks>
        public event EventHandler<PropertyChangingEventArgs> PropertyChanging;
        protected virtual bool OnPropertyChanging(PropertyChangingEventArgs e)
        {
            PropertyChanging.Raise(this, e);
            return !e.Cancel;
        }

        #endregion

        #region Misc

        /// <summary>
        /// Removes the object from its manager.
        /// </summary>
        public void Remove()
        {
            Manager.Objects.Remove(this);
        }

        /// <summary>
        /// Public for tests, don't use in real code
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="identifier"></param>
        internal void Initialize(ObjectManager owner, ObjectIdentifier identifier)
        {
            Debug.Assert(m_manager == null && owner != null, "Can only be initialized once.");
            m_manager = owner;
            m_identifier = identifier;
        }

        #endregion

        #endregion
    }
}
