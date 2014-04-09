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
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Mox
{
    /// <summary>
    /// Base class for properties.
    /// </summary>
    public abstract class PropertyBase
    {
        #region Variables

        private static readonly Dictionary<Type, ObjectTypeInfo> ms_propertiesByType = new Dictionary<Type, ObjectTypeInfo>();
        private static readonly Dictionary<PropertyKey, PropertyBase> ms_allProperties = new Dictionary<PropertyKey, PropertyBase>();

        private readonly string m_name;
        private readonly FieldInfo m_backingField;
        private readonly PropertyFlags m_flags;

        private readonly IManipulator m_manipulator;

        #endregion

        #region Constructor

        internal PropertyBase(string name, FieldInfo backingField, PropertyFlags flags)
        {
            Throw.IfEmpty(name, "name");
            Throw.IfNull(backingField, "backingField");
            Throw.IfNull(flags, "flags");

            m_name = name;
            m_backingField = backingField;
            m_flags = flags;

            m_manipulator = new PropertyManipulator(this);

            Register();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Name of the property.
        /// </summary>
        public string Name
        {
            get { return m_name; }
        }

        public FieldInfo BackingField
        {
            get { return m_backingField; }
        }

        /// <summary>
        /// Owner Type.
        /// </summary>
        public Type OwnerType
        {
            get { return m_backingField.DeclaringType; }
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
            get;
            internal set;
        }

        /// <summary>
        /// Type of the value.
        /// </summary>
        public Type ValueType
        {
            get { return m_backingField.FieldType; }
        }

        /// <summary>
        /// Whether the property is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return m_backingField.IsInitOnly; }
        }

        /// <summary>
        /// Whether the property can be affected by effects.
        /// </summary>
        public bool IsModifiable
        {
            get { return (m_flags & PropertyFlags.Modifiable) == PropertyFlags.Modifiable; }
        }

        internal IManipulator Manipulator
        {
            get { return m_manipulator; }
        }

        #endregion

        #region Methods

        public static IEnumerable<PropertyBase> GetAllProperties(Type type)
        {
            for (; type != null; type = type.BaseType)
            {
                ObjectTypeInfo info;
                if (ms_propertiesByType.TryGetValue(type, out info))
                {
                    foreach (var property in info.Properties)
                        yield return property;
                }
            }
        }

        public static PropertyBase GetProperty(Type ownerType, string name)
        {
            Debug.Assert(ownerType != null);
            Debug.Assert(!string.IsNullOrEmpty(name));

            return ms_allProperties.SafeGetValue(new PropertyKey { OwnerType = ownerType, Name = name });
        }

        internal static void InitializeDefaultValues(object instance)
        {
            var type = instance.GetType();

            for (; type != null; type = type.BaseType)
            {
                ObjectTypeInfo info;
                if (ms_propertiesByType.TryGetValue(type, out info))
                {
                    info.InitializeDefaultValues(instance);
                }
            }
        }

        private void Register()
        {
            ObjectTypeInfo info;
            if (!ms_propertiesByType.TryGetValue(OwnerType, out info))
            {
                info = new ObjectTypeInfo();
                ms_propertiesByType.Add(OwnerType, info);
            }
            info.Properties.Add(this);

            ms_allProperties.Add(new PropertyKey { OwnerType = OwnerType, Name = Name }, this);
        }

        public override string ToString()
        {
            return string.Format("[Property {0} on {1}]", Name, OwnerType.FullName);
        }

        #endregion

        #region Inner Types

        public interface IManipulator
        {
            object GetValueDirect(object instance);
            void SetValueDirect(object instance, object value);
        }

        private struct PropertyKey
        {
            public Type OwnerType;
            public string Name;
        }

        private class ObjectTypeInfo
        {
            public readonly List<PropertyBase> Properties = new List<PropertyBase>();
            private int m_initialized;

            public void InitializeDefaultValues(object instance)
            {
                if (Interlocked.CompareExchange(ref m_initialized, 1, 0) == 0)
                {
                    foreach (var property in Properties)
                    {
                        property.DefaultValue = property.Manipulator.GetValueDirect(instance);
                    }
                }
            }
        }

        private class PropertyManipulator : PropertyBase.IManipulator
        {
            private delegate object GetValueDirectDelegate(object instance);
            private delegate void SetValueDirectDelegate(object instance, object value);

            private readonly GetValueDirectDelegate m_getValueDirect;
            private readonly SetValueDirectDelegate m_setValueDirect;

            public PropertyManipulator(PropertyBase property)
            {
                m_getValueDirect = Generate_GetValueDirect(property);
                m_setValueDirect = Generate_SetValueDirect(property);
            }

            public object GetValueDirect(object instance)
            {
                return m_getValueDirect(instance);
            }

            public void SetValueDirect(object instance, object value)
            {
                m_setValueDirect(instance, value);
            }

            private static GetValueDirectDelegate Generate_GetValueDirect(PropertyBase property)
            {
                // Generate something like:
                // public object GetValueDirect(object instance)
                // {
                //     return ((MyClass)instance).MyField;
                // }

                DynamicMethod method = new DynamicMethod("GetValueDirect", typeof(object), new[] { typeof(object) }, property.OwnerType, true);

                ILGenerator ilGenerator = method.GetILGenerator();
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Castclass, property.OwnerType);
                ilGenerator.Emit(OpCodes.Ldfld, property.BackingField);

                if (property.BackingField.FieldType.IsValueType)
                    ilGenerator.Emit(OpCodes.Box, property.BackingField.FieldType);

                ilGenerator.Emit(OpCodes.Ret);

                return (GetValueDirectDelegate)method.CreateDelegate(typeof(GetValueDirectDelegate));
            }

            private static SetValueDirectDelegate Generate_SetValueDirect(PropertyBase property)
            {
                // Generate something like:
                // public void SetValueDirect(object instance, object value)
                // {
                //     ((MyClass)instance).MyField = (FieldType)value;
                // }

                DynamicMethod method = new DynamicMethod("SetValueDirect", typeof(void), new[] { typeof(object), typeof(object) }, property.OwnerType, true);

                ILGenerator ilGenerator = method.GetILGenerator();
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Castclass, property.OwnerType);
                ilGenerator.Emit(OpCodes.Ldarg_1);
                ilGenerator.Emit(OpCodes.Unbox_Any, property.BackingField.FieldType);
                ilGenerator.Emit(OpCodes.Stfld, property.BackingField);
                ilGenerator.Emit(OpCodes.Ret);

                return (SetValueDirectDelegate)method.CreateDelegate(typeof(SetValueDirectDelegate));
            }
        }

        #endregion
    }

    /// <summary>
    /// The property of an object.
    /// </summary>
    public sealed class Property<T> : PropertyBase
    {
        #region Constructor

        private Property(string name, FieldInfo backingField, PropertyFlags flags)
            : base(name, backingField, flags)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers a property of the given type <typeparamref name="T"/> using the backing field returned by the expression.
        /// </summary>
        public static Property<T> RegisterProperty<TOwner>(string name, Expression<Func<TOwner, T>> backingFieldExpression, PropertyFlags flags = PropertyFlags.None)
        {
            MemberExpression body = backingFieldExpression.Body as MemberExpression;
            Throw.InvalidArgumentIf(body == null || !(body.Member is FieldInfo), "Backing field expression doesn't point to a valid field", "backingFieldExpression");
            FieldInfo fieldInfo = (FieldInfo)body.Member;
            
            Throw.InvalidArgumentIf(fieldInfo.DeclaringType != typeof(TOwner), "Backing field must be declared in the registering type", "backingFieldExpression");

            return new Property<T>(name, fieldInfo, flags);
        }

        #endregion
    }
}
