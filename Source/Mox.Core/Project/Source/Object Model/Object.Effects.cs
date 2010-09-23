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
            UpdateEffectiveValueForEffects(null, new EntryIndex(), new ValueEntry());
        }

        private void UpdateEffectiveValueForEffects(PropertyBase property, EntryIndex entryIndex, ValueEntry newEntry)
        {
            UpdateEffectiveValueForEffects(property, entryIndex, newEntry, AppliedEffects);
        }

        private void UpdateEffectiveValueForEffects(PropertyBase property, EntryIndex entryIndex, ValueEntry newEntry, IEnumerable<EffectInstance> effects)
        {
            ValueEntryStore originalEntries = m_entries.AsResolved();

            HashSet<PropertyBase> potentiallyChangedProperties = new HashSet<PropertyBase>();

            foreach (ValueEntry entry in m_entries)
            {
                if (entry.ResetToBaseValue())
                {
                    potentiallyChangedProperties.Add(PropertyBase.AllProperties[entry.PropertyIndex]);
                }
            }

            if (property != null)
            {
                potentiallyChangedProperties.Add(property);
                m_entries.SetEffectiveValue(entryIndex, newEntry);
            }

            ApplyEffects(effects);

            foreach (var appliedEffect in effects)
            {
                potentiallyChangedProperties.Add(appliedEffect.Effect.Property);
            }

            ResolveEventsAfterEffect(originalEntries, potentiallyChangedProperties);
        }

        private void ResolveEventsAfterEffect(ValueEntryStore originalEntries, IEnumerable<PropertyBase> potentiallyChangedProperties)
        {
            foreach (PropertyBase property in potentiallyChangedProperties)
            {
                ValueEntry oldEntry = originalEntries.GetValueEntry(property, RequestType.FullyResolved);
                ValueEntry newEntry = m_entries.GetValueEntry(property, RequestType.FullyResolved);

                if (!Equals(oldEntry.Value, newEntry.Value))
                {
                    OnPropertyChanged(new PropertyChangedEventArgs(this, property, oldEntry.Value, newEntry.Value));
                }
            }
        }

        private void ApplyEffects(IEnumerable<EffectInstance> effects)
        {
            foreach (var effectInstance in effects)
            {
                EffectBase effect = effectInstance.Effect;

                EntryIndex entryIndex = m_entries.LookupEntry(effect.Property.GlobalIndex);

                ValueEntry valueEntry = m_entries.GetValueEntry(entryIndex, effect.Property, RequestType.RawEntry);
                valueEntry.Modify(this, effect);
                m_entries.SetEffectiveValue(entryIndex, valueEntry);
            }
        }

        #endregion
    }
}
