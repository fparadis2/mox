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
using System.Linq;
using System.Text;

namespace Mox
{
    /// <summary>
    /// Base class for properties.
    /// </summary>
    public abstract class PropertyBase
    {
        #region Inner Types

        internal class PropertyCollection
        {
            #region Inner Types

            private class PropertyByNameCollection : KeyedCollection<string, PropertyBase>
            {
                protected override string GetKeyForItem(PropertyBase property)
                {
                    return property.Name;
                }
            }

            #endregion

            #region Variables

            private readonly Dictionary<Type, PropertyByNameCollection> m_properties = new Dictionary<Type, PropertyByNameCollection>();
            private readonly Dictionary<int, PropertyBase> m_propertiesByIndex = new Dictionary<int, PropertyBase>();

            #endregion

            #region Properties

            public PropertyBase this[int globalIndex]
            {
                get { return m_propertiesByIndex.SafeGetValue(globalIndex); }
            }

            #endregion

            #region Methods

            public KeyedCollection<string, PropertyBase> GetPropertiesByName(Type type)
            {
                PropertyByNameCollection collection;
                if (m_properties.TryGetValue(type, out collection))
                {
                    return collection;
                }
                return new PropertyByNameCollection();
            }

            public PropertyBase GetProperty(Type type, string name)
            {
                PropertyByNameCollection collection;
                if (m_properties.TryGetValue(type, out collection))
                {
                    if (collection.Contains(name))
                    {
                        return collection[name];
                    }
                }

                return null;
            }

            public void Add(PropertyBase property)
            {
                PropertyByNameCollection collection;
                if (!m_properties.TryGetValue(property.OwnerType, out collection))
                {
                    collection = new PropertyByNameCollection();
                    m_properties.Add(property.OwnerType, collection);
                }
                collection.Add(property);

                m_propertiesByIndex.Add(property.GlobalIndex, property);
            }

            #endregion
        }

        #endregion

        #region Constants

        public const int InvalidPropertyIndex = 0;

        #endregion

        #region Variables

        internal static readonly PropertyCollection AllProperties = new PropertyCollection();
        private static int m_nextIndex = InvalidPropertyIndex + 1;

        private readonly int m_globalIndex;
        private readonly string m_name;
        private readonly Type m_ownerType;
        private readonly PropertyFlags m_flags;
        private readonly object m_defaultValue;

        #endregion

        #region Constructor

        internal PropertyBase(string name, Type ownerType, PropertyFlags flags, object defaultValue)
        {
            Throw.IfEmpty(name, "name");
            Throw.IfNull(ownerType, "ownerType");
            Throw.IfNull(flags, "flags");

            m_globalIndex = AllocateNextIndex();
            m_name = name;
            m_ownerType = ownerType;
            m_flags = flags;
            m_defaultValue = defaultValue;

            AllProperties.Add(this);
        }

        #endregion

        #region Properties

        public int GlobalIndex
        {
            get { return m_globalIndex; }
        }

        /// <summary>
        /// Name of the property.
        /// </summary>
        public string Name
        {
            get { return m_name; }
        }

        /// <summary>
        /// Owner Type.
        /// </summary>
        public Type OwnerType
        {
            get { return m_ownerType; }
        }

        /// <summary>
        /// Flags.
        /// </summary>
        public PropertyFlags Flags
        {
            get { return m_flags; }
        }

        /// <summary>
        /// Default Value.
        /// </summary>
        public object DefaultValue
        {
            get { return m_defaultValue; }
        }

        /// <summary>
        /// Type of the value.
        /// </summary>
        public abstract Type ValueType
        {
            get;
        }

        /// <summary>
        /// Whether the property is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return (m_flags & PropertyFlags.ReadOnly) == PropertyFlags.ReadOnly; }
        }

        /// <summary>
        /// Whether the property can be affected by effects.
        /// </summary>
        public bool IsModifiable
        {
            get { return (m_flags & PropertyFlags.Modifiable) == PropertyFlags.Modifiable; }
        }

        #endregion

        #region Methods

        private static int AllocateNextIndex()
        {
            return m_nextIndex++;
        }

        public override string ToString()
        {
            return string.Format("[Property {0} on {1}]", Name, OwnerType.FullName);
        }

        #endregion
    }

    /// <summary>
    /// The property of an object.
    /// </summary>
    public sealed class Property<T> : PropertyBase
    {
        #region Constructor

        private Property(string name, Type ownerType, PropertyFlags flags, T defaultValue)
            : base(name, ownerType, flags, defaultValue)
        {
        }

        #endregion

        #region Properties

        public override Type ValueType
        {
            get { return typeof(T); }
        }

        #endregion

        #region Methods

        #region Property registration

        /// <summary>
        /// Registers a property of the given type <typeparamref name="T"/> on the given <paramref name="ownerType"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Property<T> RegisterProperty(string name, Type ownerType)
        {
            return RegisterProperty(name, ownerType, PropertyFlags.None);
        }

        /// <summary>
        /// Registers a property of the given type <typeparamref name="T"/> on the given <paramref name="ownerType"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Property<T> RegisterProperty(string name, Type ownerType, PropertyFlags flags)
        {
            return RegisterProperty(name, ownerType, flags, default(T));
        }

        /// <summary>
        /// Registers a property of the given type <typeparamref name="T"/> on the given <paramref name="ownerType"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Property<T> RegisterProperty(string name, Type ownerType, PropertyFlags flags, T defaultValue)
        {
            return new Property<T>(name, ownerType, flags, defaultValue);
        }

        /// <summary>
        /// Registers a property of the given type <typeparamref name="T"/> on the given <paramref name="ownerType"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Property<T> RegisterAttachedProperty(string name, Type ownerType)
        {
            return RegisterAttachedProperty(name, ownerType, PropertyFlags.None);
        }

        /// <summary>
        /// Registers a property of the given type <typeparamref name="T"/> on the given <paramref name="ownerType"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Property<T> RegisterAttachedProperty(string name, Type ownerType, PropertyFlags flags)
        {
            return RegisterAttachedProperty(name, ownerType, flags, default(T));
        }

        /// <summary>
        /// Registers a property of the given type <typeparamref name="T"/> on the given <paramref name="ownerType"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Property<T> RegisterAttachedProperty(string name, Type ownerType, PropertyFlags flags, T defaultValue)
        {
            return new Property<T>(name, ownerType, flags | PropertyFlags.Attached, defaultValue);
        }

        #endregion

        #endregion
    }
}
