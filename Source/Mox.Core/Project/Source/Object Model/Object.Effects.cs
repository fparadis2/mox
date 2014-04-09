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
using Mox.Collections;

namespace Mox
{
    partial class Object
    {
        #region Variables

        private readonly ModifiedValuesStorage m_modifiedValuesStorage = new ModifiedValuesStorage();
        private readonly SortedArray<EffectInstance> m_localEffects = new SortedArray<EffectInstance>();

        #endregion

        #region Properties

        public ICollection<EffectInstance> AppliedEffects
        {
            get { return m_localEffects; }
        }

        #endregion

        #region Methods

        internal void InvalidateEffects()
        {
            UpdateEffectiveValueForEffects(null, null);
        }

        private void UpdateEffectiveValueForEffects(PropertyBase property, object newBaseValue)
        {
            PotentiallyChangedProperties potentiallyChangedProperties = new PotentiallyChangedProperties();
            m_modifiedValuesStorage.Fill(potentiallyChangedProperties, this);

            if (property != null)
            {
                potentiallyChangedProperties.ConsiderProperty(property, this);
            }

            m_modifiedValuesStorage.Reset(this);

            if (property != null)
            {
                property.Manipulator.SetValueDirect(this, newBaseValue);
            }

            foreach (var effectInstance in AppliedEffects)
            {
                potentiallyChangedProperties.ConsiderProperty(effectInstance.Effect.Property, this);
                m_modifiedValuesStorage.ApplyEffect(this, effectInstance);
            }

            potentiallyChangedProperties.ResolveEvents(this);
        }

        #endregion

        #region  Inner Types

        internal class PotentiallyChangedProperties
        {
            private readonly Dictionary<PropertyBase, object> m_values = new Dictionary<PropertyBase, object>();

            public void ConsiderProperty(PropertyBase property, Object instance)
            {
                if (!m_values.ContainsKey(property))
                {
                    m_values.Add(property, property.Manipulator.GetValueDirect(instance));
                }
            }

            public void Add(PropertyBase property, object originalValue)
            {
                m_values.Add(property, originalValue);
            }

            public void ResolveEvents(Object instance)
            {
                foreach (var pair in m_values)
                {
                    object oldValue = pair.Value;
                    object newValue = pair.Key.Manipulator.GetValueDirect(instance);

                    if (!Equals(oldValue, newValue))
                    {
                        instance.OnPropertyChanged(new PropertyChangedEventArgs(instance, pair.Key, oldValue, newValue));
                    }
                }
            }
        }

        internal class ModifiedValuesStorage
        {
            private readonly Dictionary<PropertyBase, object> m_entries = new Dictionary<PropertyBase, object>();

            public ICollection<PropertyBase> ModifiedProperties
            {
                get { return m_entries.Keys; }
            }

            public object GetOriginalValue(PropertyBase property, Object instance)
            {
                object originalValue;
                if (m_entries.TryGetValue(property, out originalValue))
                    return originalValue;

                return property.Manipulator.GetValueDirect(instance);
            }

            public void Fill(PotentiallyChangedProperties potentiallyChangedProperties, Object instance)
            {
                foreach (var pair in m_entries)
                {
                    potentiallyChangedProperties.ConsiderProperty(pair.Key, instance);
                }
            }

            public void Reset(Object instance)
            {
                foreach (var pair in m_entries)
                {
                    pair.Key.Manipulator.SetValueDirect(instance, pair.Value);
                }

                m_entries.Clear();
            }

            public void ApplyEffect(Object instance, EffectInstance effectInstance)
            {
                EffectBase effect = effectInstance.Effect;

                object value = effect.Property.Manipulator.GetValueDirect(instance);

                if (!m_entries.ContainsKey(effect.Property))
                {
                    // Store original value
                    m_entries.Add(effect.Property, value);
                }

                value = effect.ModifyInternal(instance, value);
                effect.Property.Manipulator.SetValueDirect(instance, value);
            }
        }

        #endregion
    }
}
