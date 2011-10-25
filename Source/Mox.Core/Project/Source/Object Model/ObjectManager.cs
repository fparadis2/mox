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
using System.Threading;

using Mox.Transactions;

using ObjectIdentifier = System.Int32;

namespace Mox
{
    /// <summary>
    /// Manages objects... duh
    /// </summary>
    public partial class ObjectManager
    {
        #region Constants

        /// <summary>
        /// Invalid object identifier.
        /// </summary>
        public const int InvalidIdentifier = 0;

        #endregion

        #region Variables

        private readonly ObjectCollection m_internalObjects;
        private readonly TransactionableObjectCollection m_objects;
        private readonly Dictionary<ObjectIdentifier, Object> m_objectsByIdentifier = new Dictionary<ObjectIdentifier, Object>();
        private IObjectController m_objectController;

        private readonly IList<EffectInstance> m_effects;

        private int m_threadId = -1;
        private int m_nextIdentifier = InvalidIdentifier + 1;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new object manager.
        /// </summary>
        public ObjectManager()
        {
            m_objectController = new ObjectController(this);
            m_internalObjects = new ObjectCollection(this);
            m_objects = new TransactionableObjectCollection(m_internalObjects);

            m_effects = RegisterInventory<EffectInstance>();
        }

        #endregion

        #region Properties

        public IObjectController Controller
        {
            get { return m_objectController; }
        }

        /// <summary>
        /// Objects managed by this manager.
        /// </summary>
        public IObjectCollection Objects
        {
            get { return m_objects; }
        }

        /// <summary>
        /// Whether events should be triggered.
        /// </summary>
        public bool IsMaster
        {
            get
            {
#warning TODO
                return true;
                //return ControlMode == ReplicationControlMode.Master && !TransactionStack.IsRollbacking;
            }
        }

        #endregion

        #region Methods

        #region Object management

        /// <summary>
        /// Creates a new object of type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// All object creation must pass through this method.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Create<T>()
            where T : Object, new()
        {
            CreateObjectCommand<T> creationCommand = new CreateObjectCommand<T>();
            Controller.Execute(creationCommand);
            return GetObjectByIdentifier<T>(creationCommand.ObjectIdentifier);
        }

        private void CreateImpl<T>(ObjectIdentifier identifier)
            where T : Object, new()
        {
            T obj = new T();
            obj.Initialize(this, identifier);
            AddObjectToMap(obj);
        }

        private void AddObject(ObjectIdentifier identifier)
        {
            Debug.Assert(m_objectsByIdentifier.ContainsKey(identifier), "Synchronisation problem. Did you previously remove this object?");
            Object obj = m_objectsByIdentifier[identifier];
            m_internalObjects.Add(obj);
            obj.Init();
        }

        private Object RemoveObject(ObjectIdentifier identifier)
        {
            Debug.Assert(m_objectsByIdentifier.ContainsKey(identifier), "Synchronisation problem");
            Object obj = m_objectsByIdentifier[identifier];
            Debug.Assert(m_internalObjects.Contains(obj));
            obj.Uninit();
            m_internalObjects.Remove(obj);
            return obj;
        }

        private void AddObjectToMap(Object obj)
        {
            m_objectsByIdentifier.Add(obj.Identifier, obj);
        }

        private void RemoveObjectFromMap(ObjectIdentifier identifier)
        {
            bool result = m_objectsByIdentifier.Remove(identifier);
            Debug.Assert(result);
        }

        private void ClearObjects()
        {
            foreach (Object obj in m_internalObjects)
            {
                obj.Uninit();
            }

            m_internalObjects.Clear();
        }

        public T GetObjectByIdentifier<T>(ObjectIdentifier identifier)
            where T : IObject
        {
            return (T)(object)m_objectsByIdentifier[identifier];
        }

        public bool ContainsObject(ObjectIdentifier identifier)
        {
            return m_objectsByIdentifier.ContainsKey(identifier);
        }

        #endregion

        #region Property management

        /// <summary>
        /// Sets the value of the given <paramref name="property"/>, ignoring the "ReadOnly" metadata flag on the property if necessary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Object to set the value of.</param>
        /// <param name="property">Property to set the value of.</param>
        /// <param name="value">Value to set.</param>
        protected void SetObjectValue<T>(Object obj, Property<T> property, T value)
        {
            Throw.IfNull(obj, "obj");
            Throw.IfNull(property, "property");
            Throw.InvalidArgumentIf(obj.Manager != this, "Object is not owned by this manager", "obj");

            obj.SetValue(property, value, Object.Situation.Created);
        }

        #endregion

        #region Misc

        public IDisposable EnforceThreadAffinity()
        {
            Throw.InvalidOperationIf(m_threadId != -1, "Already in thread affine context");
            m_threadId = Thread.CurrentThread.ManagedThreadId;

            return new DisposableHelper(() => { m_threadId = -1; });
        }

        [Conditional("DEBUG")]
        internal void ValidateThread()
        {
            if (m_threadId != -1)
            {
                Throw.InvalidProgramIf(m_threadId != Thread.CurrentThread.ManagedThreadId, "Cannot access an object from another thread from which it was created");
            }
        }

        #endregion

        #region Effects

        #region Local

        public LocalEffectInstance CreateLocalEffect(Object affectedObject, EffectBase effect)
        {
            return CreateLocalEffect(affectedObject, effect, null);
        }

        public LocalEffectInstance CreateScopedLocalEffect<TObjectScope>(Object affectedObject, EffectBase effect)
            where TObjectScope : IObjectScope, new()
        {
            return CreateLocalEffect(affectedObject, effect, typeof (TObjectScope));
        }

        private LocalEffectInstance CreateLocalEffect(Object affectedObject, EffectBase effect, Type objectScopeType)
        {
            Throw.IfNull(affectedObject, "affectedObject");

            return CreateEffect<LocalEffectInstance>(effect, objectScopeType, e => SetObjectValue(e, LocalEffectInstance.AffectedObjectProperty, affectedObject));
        }

        #endregion

        protected TEffectInstance CreateEffect<TEffectInstance>(EffectBase effect, Type objectScopeType, Action<TEffectInstance> initialization)
            where TEffectInstance : EffectInstance, new()
        {
            Throw.IfNull(effect, "effect");
            Debug.Assert(objectScopeType == null || typeof(IObjectScope).IsAssignableFrom(objectScopeType));

            using (Controller.BeginTransaction())
            {
                TEffectInstance effectInstance = Create<TEffectInstance>();

                if (objectScopeType != null)
                {
                    SetObjectValue(effectInstance, Object.ScopeTypeProperty, objectScopeType);
                }

                SetObjectValue(effectInstance, EffectInstance.EffectProperty, effect);
                initialization(effectInstance);

                Objects.Add(effectInstance);
                return effectInstance;
            }
        }

        private void InvalidateEffects(Object sender, PropertyBase property)
        {
            HashSet<Object> objectsToInvalidate = new HashSet<Object>();

            foreach (var effectInstance in m_effects)
            {
                foreach (var objectToInvalidate in effectInstance.Invalidate(sender, property))
                {
                    objectsToInvalidate.Add(objectToInvalidate);
                }
            }

            objectsToInvalidate.ForEach(o => o.InvalidateEffects());
        }

        #endregion

        #region Controller

        internal IDisposable UpgradeController(IObjectController objectController)
        {
            var oldController = m_objectController;
            m_objectController = objectController;
            return new DisposableHelper(() => m_objectController = oldController);
        }

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Triggered when the value of a property on this object changes.
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs> PropertyChanged;
        protected internal virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged.Raise(this, e);
            OnAfterPropertyChanged(e.Object, e.Property);
        }

        internal void OnAfterPropertyChanged(Object sender, PropertyBase property)
        {
            InvalidateEffects(sender, property);
        }

        #endregion
    }
}
