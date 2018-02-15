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

namespace Mox.Abilities
{
    /// <summary>
    /// A static ability that has a continuous effect as long as its source is in play.
    /// </summary>
    public class ContinuousAbility : StaticAbility, ISpellContext
    {
        #region Variables

        //private static readonly Property<int[]> AttachedEffectsProperty = Property<int[]>.RegisterProperty("AttachedEffects", typeof(ContinuousAbility));
        private IEnumerable<Object> m_attachedEffects;

        #endregion

        #region Properties

        private IEnumerable<Object> AttachedEffects
        {
            get
            {
                return m_attachedEffects ?? new Object[0];
                //int[] attachedEffectsIds = GetValue(AttachedEffectsProperty);
                //return attachedEffectsIds == null ?
                //    new Object[0] :
                //    attachedEffectsIds.Select(i => Manager.GetObjectByIdentifier<Object>(i));
            }
            set
            {
                m_attachedEffects = value;
                //SetValue(AttachedEffectsProperty, value == null ? null : value.Select(e => e.Identifier).ToArray());
            }
        }

        Spell2 ISpellContext.Spell => null;
        Ability ISpellContext.Ability => this;
        Player ISpellContext.Controller => Controller;

        #endregion

        #region Methods

        protected override void Init()
        {
            base.Init();

            Source.PropertyChanged += Source_PropertyChanged;

            if (Source.Zone == Manager.Zones.Battlefield && Manager.IsMaster)
            {
                AddAllEffects();
            }
        }

        protected override void Uninit()
        {
            Source.PropertyChanged -= Source_PropertyChanged;

            base.Uninit();
        }

        private void AddAllEffects()
        {
            Debug.Assert(!AttachedEffects.Any());

            var spellDefinition = SpellDefinition;
            Debug.Assert(spellDefinition.Costs.Count == 0, "Attachment abilities are not supposed to have costs");

            List<Object> attachedEffectInstances = new List<Object>();

            foreach (var action in spellDefinition.Actions)
            {
                Debug.Assert(action is ApplyEffectAction);
                ApplyEffectAction applyEffectAction = (ApplyEffectAction)action;

                var effect = applyEffectAction.CreateEffect(this);
                var instance = Game.CreateTrackingEffect(this, applyEffectAction.Targets, effect, null);
                attachedEffectInstances.Add(instance);
            }

            AttachedEffects = attachedEffectInstances;
        }

        private void RemoveAllEffects()
        {
            foreach (Object instance in AttachedEffects)
            {
                instance.Remove();
            }

            AttachedEffects = null;
        }

        #endregion

        #region Event Handlers

        void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.Property == Card.ZoneIdProperty && Manager.IsMaster)
            {
                RemoveAllEffects();

                if (Source.ZoneId == Zone.Id.Battlefield)
                {
                    AddAllEffects();
                }
            }
        }

        #endregion
    }
}
