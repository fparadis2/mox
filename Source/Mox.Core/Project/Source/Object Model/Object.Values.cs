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
using System.Linq;
using Mox.Transactions;

namespace Mox
{
    partial class Object
    {
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
            return (T)GetValue((PropertyBase)property);
        }

        /// <summary>
        /// Gets the value of the given <paramref name="property"/> for the current object.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public object GetValue(PropertyBase property)
        {
            Manager.ValidateThread();
            ValidateProperty(property);
            return property.Manipulator.GetValueDirect(this);
        }

        private object GetBaseValue(PropertyBase property)
        {
            return m_modifiedValuesStorage.GetOriginalValue(property, this);
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

        public void SetValue<T>(Property<T> property, T value, ref T dummy)
        {
            SetValue(property, value);
        }

        internal void SetValue<T>(Property<T> property, T value, Situation situation)
        {
            Manager.ValidateThread();
            ValidateProperty(property);
            Throw.InvalidOperationIf(situation != Situation.Created && property.IsReadOnly, "Property is read-only");
            ValidateSituation(situation);

            SetValue((PropertyBase)property, value, situation);
        }

        private void SetValue(PropertyBase property, object valueToSet, Situation situation)
        {
            ISetValueAdapter adapter = GetAdapter(valueToSet, property.BackingField.FieldType);
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

        private void SetValueInternal(PropertyBase property, object newBaseValue)
        {
            object oldBaseValue = GetBaseValue(property);

            if (!OnPropertyChanging(new PropertyChangingEventArgs(this, property, oldBaseValue, newBaseValue)))
                return;

            // Fast path for non-modifiable property or non-modified objects
            if (!property.IsModifiable || !AppliedEffects.Any())
            {
                property.Manipulator.SetValueDirect(this, newBaseValue);
                OnPropertyChanged(new PropertyChangedEventArgs(this, property, oldBaseValue, newBaseValue));
            }
            else
            {
                UpdateEffectiveValueForEffects(property, newBaseValue);
            }
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

            SetValue(property, property.DefaultValue, Situation.Added);
        }

        /// <summary>
        /// Resets the value of all properties on this object, except for read-only properties.
        /// </summary>
        public void ResetAllValues()
        {
            Manager.ValidateThread();

            using (ObjectController.BeginCommandGroup())
            {
                var allProperties = PropertyBase.GetAllProperties(GetType());

                foreach (var property in allProperties)
                {
                    if (!property.IsReadOnly)
                    {
                        SetValue(property, property.DefaultValue, Situation.Added);
                    }
                }
            }
        }

#warning [High] TODO: CODEGEN Object Model 2.0
        public IEnumerable<object> GetAllValues()
        {
            Manager.ValidateThread();

            var allProperties = PropertyBase.GetAllProperties(GetType());

            foreach (var property in allProperties)
            {
                yield return property.Manipulator.GetValueDirect(this);
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

            foreach (var property in PropertyBase.GetAllProperties(GetType()))
            {
                if (ignoredProperties.Contains(property))
                    continue;

                if (!Equals(property.Manipulator.GetValueDirect(this), property.Manipulator.GetValueDirect(other)))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Utilities

        [Conditional("DEBUG")]
        private void ValidateProperty(PropertyBase property)
        {
            Throw.IfNull(property, "property");

            if (!property.OwnerType.IsAssignableFrom(GetType()))
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
