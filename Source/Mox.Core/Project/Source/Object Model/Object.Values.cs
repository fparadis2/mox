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
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Mox.Transactions;

namespace Mox
{
    partial class Object
    {
        #region Inner Types

        [Flags]
        private enum ValueFlags
        {
            None = 0,
            HasModifiers = 1,
        }

        private enum RequestType
        {
            RawEntry = 0,
            FullyResolved = 1
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ValueEntry
        {
            #region Constants

            internal static readonly object UnsetValue = new object();

            #endregion

            #region Variables

            private readonly int m_propertyIndex;
            private object m_value;
            private ValueFlags m_flags;

            #endregion

            #region Constructor

            public ValueEntry(PropertyBase property)
                : this(property.GlobalIndex)
            {
            }

            private ValueEntry(int propertyIndex)
            {
                m_propertyIndex = propertyIndex;
                m_value = UnsetValue;
                m_flags = ValueFlags.None;
            }

            #endregion

            #region Properties

            private ValueFlags Flags
            {
                get { return m_flags; }
                set { m_flags = value; }
            }

            public bool IsSet
            {
                get { return !ReferenceEquals(Value, UnsetValue); }
            }

            public int PropertyIndex
            {
                get { return m_propertyIndex; }
            }

            public object Value
            {
                get { return m_value; }
                set { m_value = value; }
            }

            internal object BaseValue
            {
                get 
                {
                    if (HasModifiers)
                    {
                        ModifiedValueContainer modifier = (ModifiedValueContainer)Value;
                        return modifier.BaseValue;
                    }

                    return Value;
                }
            }

            private bool HasModifiers
            {
                get 
                {
                    return (m_flags & ValueFlags.HasModifiers) == ValueFlags.HasModifiers;
                }
            }

            #endregion

            #region Methods

            internal static ValueEntry CreateDefaultValueEntry(PropertyBase property, object value)
            {
                return new ValueEntry(property) { Value = value };
            }

            /// <summary>
            /// Gets a value entry that contains the final value of the property.
            /// </summary>
            /// <returns></returns>
            internal ValueEntry Resolve()
            {
                Debug.Assert(PropertyIndex != PropertyBase.InvalidPropertyIndex);

                if (HasModifiers)
                {
                    ModifiedValueContainer modifier = (ModifiedValueContainer)Value;
                    return new ValueEntry(m_propertyIndex)
                    {
                        Value = modifier.ModifiedValue,
                        Flags = ValueFlags.None
                    };
                }

                return this;
            }

            internal void Modify(Object owner, EffectBase effect)
            {
                Debug.Assert(PropertyIndex != PropertyBase.InvalidPropertyIndex);

                ModifiedValueContainer container = EnsureModifiedValue();
                container.ModifiedValue = effect.ModifyInternal(owner, container.ModifiedValue);
            }

            private ModifiedValueContainer EnsureModifiedValue()
            {
                ModifiedValueContainer container = m_value as ModifiedValueContainer;
                if (container == null)
                {
                    container = new ModifiedValueContainer
                    {
                        BaseValue = m_value,
                        ModifiedValue = m_value
                    };
                    Value = container;
                    Flags |= ValueFlags.HasModifiers;
                }

                return container;
            }

            public bool ResetToBaseValue()
            {
                Debug.Assert(PropertyIndex != PropertyBase.InvalidPropertyIndex);

                if (HasModifiers)
                {
                    ModifiedValueContainer container = (ModifiedValueContainer)Value;
                    bool wasModified = !Equals(container.ModifiedValue, container.BaseValue);
                    container.ModifiedValue = container.BaseValue;
                    return wasModified;
                }

                return false;
            }

            #endregion
        }

        private class ModifiedValueContainer
        {
            public object BaseValue;
            public object ModifiedValue;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EntryIndex
        {
            #region Constants

            private const uint FoundMask = 0x80000000;

            #endregion

            #region Variables

            private readonly uint m_store;

            #endregion

            #region Constructor

            public EntryIndex(uint index, bool found = true)
            {
                Debug.Assert((index & FoundMask) == 0, "Uh oh, out of indices!");

                m_store = index;
                if (found)
                {
                    m_store |= 0x80000000;
                }
            }

            #endregion

            #region Properties

            public bool Found
            {
                get
                {
                    return ((m_store & FoundMask) != 0);
                }
            }

            public uint Index
            {
                get
                {
                    return (m_store & ~FoundMask);
                }
            }

            #endregion
        }

        private class ValueEntryStore : IEnumerable<ValueEntry>
        {
            #region Constants

            private const uint EntriesInitialSize = 3;

            #endregion

            #region Variables

            private ValueEntry[] m_entries;
            private uint m_effectiveValueCount;

            #endregion

            #region Constructor

            public ValueEntryStore()
            {
            }

            private ValueEntryStore(ValueEntryStore other)
            {
                if (other.m_entries != null)
                {
                    m_entries = new ValueEntry[other.m_entries.Length];

                    for (int i = 0; i < other.m_effectiveValueCount; i++)
                    {
                        m_entries[i] = other.m_entries[i].Resolve();
                    }
                }

                m_effectiveValueCount = other.m_effectiveValueCount;
            }

            #endregion

            #region Methods

            public ValueEntry GetValueEntry(PropertyBase property, RequestType requestType)
            {
                return GetValueEntry(LookupEntry(property.GlobalIndex), property, requestType);
            }

            public ValueEntry GetValueEntry(EntryIndex entryIndex, PropertyBase property, RequestType requestType)
            {
                ValueEntry entry = entryIndex.Found ? m_entries[entryIndex.Index] : new ValueEntry(property);

                if (!entry.IsSet)
                {
                    return ValueEntry.CreateDefaultValueEntry(property, property.DefaultValue);
                }

                switch (requestType)
                {
                    case RequestType.RawEntry:
                        return entry;

                    case RequestType.FullyResolved:
                        return entry.Resolve();

                    default:
                        throw new NotImplementedException();
                }
            }

            public EntryIndex LookupEntry(int targetIndex)
            {
                // Binary search the correct index
                uint index = 0;
                uint effectiveValueCount = m_effectiveValueCount;

                if (effectiveValueCount > 0)
                {
                    int propertyIndex;
                    while (effectiveValueCount - index > 3)
                    {
                        uint midpoint = (effectiveValueCount + index) / 2;

                        propertyIndex = m_entries[midpoint].PropertyIndex;
                        if (targetIndex == propertyIndex)
                        {
                            return new EntryIndex(midpoint);
                        }

                        if (targetIndex <= propertyIndex)
                        {
                            effectiveValueCount = midpoint;
                        }
                        else
                        {
                            index = midpoint + 1;
                        }
                    }

                    while (index < effectiveValueCount)
                    {
                        propertyIndex = m_entries[index].PropertyIndex;
                        if (targetIndex == propertyIndex)
                        {
                            return new EntryIndex(index);
                        }

                        if (propertyIndex > targetIndex)
                        {
                            break;
                        }

                        index++;
                    }

                    return new EntryIndex(index, false);
                }

                return new EntryIndex(0, false);
            }

            public void SetEffectiveValue(EntryIndex entryIndex, ValueEntry newEntry)
            {
                if (entryIndex.Found)
                {
                    m_entries[entryIndex.Index] = newEntry;
                }
                else
                {
                    InsertEntry(entryIndex.Index, newEntry);
                }
            }

            private void InsertEntry(uint entryIndex, ValueEntry entry)
            {
                uint effectiveValuesCount = m_effectiveValueCount;
                if (effectiveValuesCount > 0)
                {
                    if (m_entries.Length == effectiveValuesCount)
                    {
                        int newLength = (int)(effectiveValuesCount * 1.2);
                        if (newLength == effectiveValuesCount)
                        {
                            newLength++;
                        }
                        ValueEntry[] destinationArray = new ValueEntry[newLength];
                        Array.Copy(m_entries, 0, destinationArray, 0, entryIndex);
                        destinationArray[entryIndex] = entry;
                        Array.Copy(m_entries, entryIndex, destinationArray, entryIndex + 1, effectiveValuesCount - entryIndex);
                        m_entries = destinationArray;
                    }
                    else
                    {
                        Array.Copy(m_entries, entryIndex, m_entries, entryIndex + 1, effectiveValuesCount - entryIndex);
                        m_entries[entryIndex] = entry;
                    }
                }
                else
                {
                    if (m_entries == null)
                    {
                        m_entries = new ValueEntry[EntriesInitialSize];
                    }
                    m_entries[0] = entry;
                }
                m_effectiveValueCount = effectiveValuesCount + 1;
            }

            public bool IsEquivalent(ValueEntryStore other, IEnumerable<PropertyBase> ignoredProperties)
            {
                int i1 = 0;
                int i2 = 0;

                while (i1 < m_effectiveValueCount && i2 < other.m_effectiveValueCount)
                {
                    ValueEntry entry1 = m_entries[i1];
                    ValueEntry entry2 = other.m_entries[i2];

                    int compare = entry1.PropertyIndex.CompareTo(entry2.PropertyIndex);
                    if (compare == 0)
                    {
                        if (!IsIgnored(ignoredProperties, entry1))
                        {
                            if (entry1.IsSet != entry2.IsSet)
                            {
                                return false;
                            }

                            if (entry1.IsSet && !Equals(entry1.Resolve().Value, entry2.Resolve().Value))
                            {
                                return false;
                            }
                        }

                        i1++;
                        i2++;
                    }
                    else
                    {
                        if (compare < 0 ? m_entries[i1++].IsSet : other.m_entries[i2++].IsSet)
                        {
                            return false;
                        }
                    }
                }

                if (!CheckRemainingPropertiesAreNotSet(m_entries, i1, m_effectiveValueCount, ignoredProperties))
                {
                    return false;
                }

                if (!CheckRemainingPropertiesAreNotSet(other.m_entries, i2, other.m_effectiveValueCount, ignoredProperties))
                {
                    return false;
                }

                return true;
            }

            private static bool CheckRemainingPropertiesAreNotSet(ValueEntry[] entries, int index, uint end, IEnumerable<PropertyBase> ignoredProperties)
            {
                while (index < end)
                {
                    ValueEntry entry = entries[index++];
                    if (entry.IsSet && !IsIgnored(ignoredProperties, entry))
                    {
                        return false;
                    }
                }

                return true;
            }

            private static bool IsIgnored(IEnumerable<PropertyBase> ignoredProperties, ValueEntry entry)
            {
                return ignoredProperties.Any(ignored => ignored.GlobalIndex == entry.PropertyIndex);
            }

            #endregion

            #region IEnumerable

            public IEnumerator<ValueEntry> GetEnumerator()
            {
                return Enumerate().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return Enumerate().GetEnumerator();
            }

            private IEnumerable<ValueEntry> Enumerate()
            {
                for (int i = 0; i < m_effectiveValueCount; i++)
                {
                    yield return m_entries[i];
                }
            }

            #endregion

            public ValueEntryStore AsResolved()
            {
                return new ValueEntryStore(this);
            }
        }

        #endregion

        #region Variables

        private readonly ValueEntryStore m_entries = new ValueEntryStore();

        #endregion

        #region Methods

        #region Get Value

        /// <summary>
        /// Gets the value of the given <paramref name="property"/> for the current object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public T GetValue<T>(Property<T> property)
        {
            Manager.ValidateThread();
            return (T)GetValueInternal(property);
        }

        private object GetValueInternal(PropertyBase property)
        {
            ValidateProperty(property);
            return m_entries.GetValueEntry(property, RequestType.FullyResolved).Value;
        }

        private object GetBaseValueInternal(PropertyBase property)
        {
            ValidateProperty(property);
            return m_entries.GetValueEntry(property, RequestType.RawEntry).BaseValue;
        }

        #endregion

        #region Set Value

        /// <summary>
        /// Sets the value of the given <paramref name="property"/> for the current object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="valueToSet"></param>
        public void SetValue<T>(Property<T> property, T valueToSet)
        {
            SetValue(property, valueToSet, Situation.Added);
        }

        internal void SetValue<T>(Property<T> property, T valueToSet, Situation situation)
        {
            Manager.ValidateThread();
            ValidateProperty(property);
            Throw.InvalidOperationIf(situation != Situation.Created && property.IsReadOnly, "Property is read-only");
            ValidateSituation(situation);

            SetValue(property, valueToSet, typeof(T), situation);
        }

        private void SetValue(PropertyBase property, object valueToSet, Type valueType, Situation situation)
        {
            ISetValueAdapter adapter = GetAdapter(valueToSet, valueType);
            ObjectController.Execute(CreateSetValueCommand(property, valueToSet, adapter, situation));
        }

        private ISetValueAdapter GetAdapter(object valueToSet, Type valueType)
        {
            ISetValueAdapter adapter;
            if (typeof(Object).IsAssignableFrom(valueType))
            {
                Throw.InvalidArgumentIf(valueToSet is Object && ((Object)valueToSet).Manager != Manager, "Value belongs to another manager", "value");
                adapter = new ObjectSetValueAdapter();
            }
            else
            {
                adapter = new NormalSetValueAdapter();
            }
            return adapter;
        }

        private ICommand CreateSetValueCommand(PropertyBase property, object valueToSet, ISetValueAdapter adapter, Situation situation)
        {
            switch (situation)
            {
                case Situation.Created:
                    return new CreationSetValueCommand(this, property, valueToSet, adapter);

                case Situation.Added:
                case Situation.Removed:
                    return CreateSetValueCommand(property, valueToSet, adapter);

                default:
                    throw new NotImplementedException();
            }
        }

        protected virtual ICommand CreateSetValueCommand(PropertyBase property, object valueToSet, ISetValueAdapter adapter)
        {
            return new SetValueCommand(this, property, valueToSet, adapter);
        }

        internal void SetValueInternal(PropertyBase property, object value)
        {
            EntryIndex entryIndex = m_entries.LookupEntry(property.GlobalIndex);
            ValueEntry newEntry = new ValueEntry(property);

            if (value != ValueEntry.UnsetValue && !Equals(value, property.DefaultValue))
            {
                newEntry.Value = value;
            }

            UpdateEffectiveValue(entryIndex, property, newEntry);
        }

        private void UpdateEffectiveValue(EntryIndex entryIndex, PropertyBase property, ValueEntry newEntry)
        {
            ValueEntry oldEntry = m_entries.GetValueEntry(entryIndex, property, RequestType.RawEntry);
            object oldValue = oldEntry.BaseValue;
            object effectiveNewValue = newEntry.IsSet ? newEntry.Value : property.DefaultValue;

            if (!OnPropertyChanging(new PropertyChangingEventArgs(this, property, oldValue, effectiveNewValue)))
            {
                return;
            }

            if (property.IsModifiable)
            {
                var appliedEffects = AppliedEffects;

                if (appliedEffects.Any())
                {
                    // Slower path for effects
                    UpdateEffectiveValueForEffects(property, entryIndex, newEntry, appliedEffects);
                    return;
                }
            }

            // Simple (fast) path for non modifiable properties
            m_entries.SetEffectiveValue(entryIndex, newEntry);
            OnPropertyChanged(new PropertyChangedEventArgs(this, property, oldValue, effectiveNewValue));
        }

        #endregion

        #region Reset Value

        /// <summary>
        /// Resets the value of the given <paramref name="property"/> for the current object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        public void ResetValue<T>(Property<T> property)
        {
            Manager.ValidateThread();
            ValidateProperty(property);
            Throw.InvalidOperationIf(property.IsReadOnly, "Property is read-only");

            SetValue(property, property.DefaultValue, typeof(T), Situation.Added);
        }

        /// <summary>
        /// Resets the value of all properties on this object, except for read-only properties.
        /// </summary>
        public void ResetAllValues()
        {
            Manager.ValidateThread();

            using (ObjectController.BeginCommandGroup())
            {
                foreach (ValueEntry valueEntry in m_entries)
                {
                    PropertyBase property = PropertyBase.AllProperties[valueEntry.PropertyIndex];
                    Debug.Assert(property != null);
                    if (!property.IsReadOnly)
                    {
                        SetValue(property, ValueEntry.UnsetValue, property.ValueType, Situation.Added);
                    }
                }
            }
        }

#warning [High] TODO: CODEGEN Object Model 2.0
        public IEnumerable<object> GetAllValues()
        {
            foreach (var valueEntry in m_entries)
            {
                if (valueEntry.IsSet)
                {
                    PropertyBase property = PropertyBase.AllProperties[valueEntry.PropertyIndex];
                    Debug.Assert(property != null);
                    if (!property.IsReadOnly)
                    {
                        yield return valueEntry.Resolve().Value;
                    }
                }
            }
        }


        #endregion

        #region IsEquivalent

        /// <summary>
        /// Returns whether this object is equivalent to the other object in terms of its property values.
        /// </summary>
        /// <remarks>
        /// Properties are only considered equivalent if they are both unset OR if they have the same set value.
        /// Objects of different types are never equivalent.
        /// </remarks>
        /// <returns></returns>
        public bool IsEquivalentTo(Object other, params PropertyBase[] ignoredProperties)
        {
            Debug.Assert(other != null);

            if (other.GetType() != GetType())
            {
                return false;
            }

            return m_entries.IsEquivalent(other.m_entries, ignoredProperties);
        }

        #endregion

        #region Utilities

        [Conditional("DEBUG")]
        private void ValidateProperty(PropertyBase property)
        {
            Throw.IfNull(property, "property");

            if ((property.Flags & PropertyFlags.Attached) == PropertyFlags.None && !property.OwnerType.IsAssignableFrom(GetType()))
            {
                throw new ArgumentException(string.Format("Property {0} can only be set on objects of type {1}", property.Name, property.OwnerType.FullName));
            }
        }

        internal enum Situation
        {
            Created, // Created but not added yet.
            Added, // Added
            Removed // Removed
        }

        [Conditional("DEBUG")]
        internal void ValidateSituation(Situation expectedSituation)
        {
            Throw.InvalidProgramIf(expectedSituation == Situation.Removed, "Can never expect to set during remove.");

            Situation currentSituation = Situation.Added;

            if (!Manager.ContainsObject(Identifier))
            {
                currentSituation = Situation.Removed;
            }

            if (!Manager.Objects.Contains(this))
            {
                currentSituation = Situation.Created;
            }

            if (currentSituation != expectedSituation)
            {
                string msg;

                switch (currentSituation)
                {
                    case Situation.Added:
                        msg = "Cannot modify an object through ObjectManager.SetObjectValue once it has been created. Use Object.SetValue instead.";
                        break;

                    case Situation.Removed:
                        msg = "Cannot modify an object while it has been removed from its manager's object list.";
                        break;

                    case Situation.Created:
                        msg = "Cannot modify an object while it is being created. Use ObjectManager.SetObjectValue instead.";
                        break;

                    default:
                        throw new NotImplementedException();
                }

                throw new InvalidOperationException(msg);
            }
        }

        #endregion

        #endregion
    }
}
