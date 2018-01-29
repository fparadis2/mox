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
using System.Diagnostics;
using System.Linq;
using ObjectIdentifier = System.Int32;

using Mox.Transactions;

namespace Mox
{
    /// <summary>
    /// Base object, allows easy serialization, transfer, transactions and replication of data.
    /// </summary>
    partial class Object
    {
        #region Inner Types

        [Serializable]
        public abstract class ObjectCommand : Command
        {
            private readonly ObjectIdentifier m_objectIdentifier;

            protected ObjectCommand(ObjectIdentifier identifier)
            {
                m_objectIdentifier = identifier;
            }

            public ObjectIdentifier ObjectIdentifier
            {
                get { return m_objectIdentifier; }
            }

            protected Object GetObject(ObjectManager manager)
            {
                return manager.GetObjectByIdentifier<Object>(ObjectIdentifier);
            }
        }

        public interface ISetValueAdapter
        {
            object GetFinalFromTransport(object transportValue, ObjectManager manager);
            object GetTransportFromFinal(object finalValue);
        }

        [Serializable]
        protected class NormalSetValueAdapter : ISetValueAdapter
        {
            #region Implementation of ISetValueAdapter<T,T>

            public object GetFinalFromTransport(object transportValue, ObjectManager manager)
            {
                return transportValue;
            }

            public object GetTransportFromFinal(object finalValue)
            {
                return finalValue;
            }

            #endregion
        }

        [Serializable]
        protected class ObjectSetValueAdapter : ISetValueAdapter
        {
            #region Implementation of ISetValueAdapter

            public object GetFinalFromTransport(object transportValue, ObjectManager manager)
            {
                ObjectIdentifier identifier = (ObjectIdentifier)transportValue;
                if (identifier == ObjectManager.InvalidIdentifier)
                {
                    return null;
                }

                Object obj = manager.GetObjectByIdentifier<Object>(identifier);
                Debug.Assert(!ReferenceEquals(obj, null), "Synchronisation problem");
                return obj;
            }

            public object GetTransportFromFinal(object finalValue)
            {
                if (ReferenceEquals(finalValue, null) || !(finalValue is Object))
                {
                    return ObjectManager.InvalidIdentifier;
                }

                return ((Object)finalValue).Identifier;
            }

            #endregion
        }

        protected static void SetValueDirect(Object obj, PropertyBase property, object oldBaseValue, object value)
        {
            property.Manipulator.SetValueDirect(obj, value);
            obj.OnPropertyChanged(new PropertyChangedEventArgs(obj, property, oldBaseValue, value));
        }

        [Serializable]
        public class SetValueCommand : ObjectCommand, ISynchronizableCommand
        {
            #region Variables

            private readonly PropertyIdentifier m_propertyIdentifier;

            private readonly ISetValueAdapter m_adapter;
            private readonly object m_newValue;
            private readonly object m_oldValue;

            #endregion

            #region Constructor

            public SetValueCommand(Object obj, PropertyBase property, object newValue, ISetValueAdapter adapter)
                : base(obj.Identifier)
            {
                Debug.Assert(property != null);
                Debug.Assert(adapter != null);

                m_propertyIdentifier = new PropertyIdentifier(property);
                m_adapter = adapter;

                m_newValue = m_adapter.GetTransportFromFinal(newValue);
                m_oldValue = m_adapter.GetTransportFromFinal(obj.GetBaseValue(property));
            }

            #endregion

            #region Properties

            public override bool IsEmpty
            {
                get
                {
                    return Equals(m_oldValue, m_newValue);
                }
            }

            protected PropertyBase Property
            {
                get
                {
                    return m_propertyIdentifier.Property;
                }
            }

            protected object NewValue
            {
                get { return m_newValue; }
            }

            #endregion

            #region Methods

            public override void Execute(ObjectManager manager)
            {
                SetValue(manager, m_adapter.GetFinalFromTransport(m_newValue, manager), true);
            }

            public override void Unexecute(ObjectManager manager)
            {
                SetValue(manager, m_adapter.GetFinalFromTransport(m_oldValue, manager), false);
            }

            private void SetValue(ObjectManager manager, object value, bool executing)
            {
                Object obj = GetObject(manager);
                object oldBaseValue = obj.GetBaseValue(Property);

                if (obj.OnPropertyChanging(new PropertyChangingEventArgs(obj, Property, oldBaseValue, value)))
                {
                    SetValue(obj, Property, oldBaseValue, value, executing);
                }
            }

            protected virtual void SetValue(Object obj, PropertyBase property, object oldBaseValue, object value, bool executing)
            {
                // Fast path for non-modifiable property or non-modified objects
                if (!property.IsModifiable || !obj.AppliedEffects.Any())
                {
                    SetValueDirect(obj, property, oldBaseValue, value);
                }
                else
                {
                    obj.UpdateEffectiveValueForEffects(property, value);
                }
            }

            protected IDisposable SuspendPropertyChangedEvents(Object obj)
            {
                return obj.SuspendPropertyChangedEvents();
            }

            #endregion

            #region Implementation of ISynchronizableCommand

            /// <summary>
            /// Object associated with the synchronizable command, if any.
            /// </summary>
            Object ISynchronizableCommand.GetObject(ObjectManager objectManager)
            {
                return GetObject(objectManager);
            }

            /// <summary>
            /// Whether this particular property should only visible to the owner of the <see cref="Object"/>.
            /// </summary>
            public bool IsPublic
            {
                get { return (Property.Flags & PropertyFlags.Private) != PropertyFlags.Private; }
            }

            /// <summary>
            /// Gets the synchronization command for this command (usually the command itself).
            /// </summary>
            public virtual ICommand Synchronize()
            {
                return this;
            }

            #endregion
        }

        [Serializable]
        protected class CreationSetValueCommand : SetValueCommand
        {
            public CreationSetValueCommand(Object obj, PropertyBase property, object newValue, ISetValueAdapter adapter)
                : base(obj, property, newValue, adapter)
            {
            }

            public override void Unexecute(ObjectManager manager)
            {
                // Do nothing.
            }
        }

        #endregion
    }
}
