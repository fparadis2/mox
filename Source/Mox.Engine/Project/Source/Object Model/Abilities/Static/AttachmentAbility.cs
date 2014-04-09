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

namespace Mox
{
    /// <summary>
    /// An ability that modifies the attachment (for auras, equipments, fortifications).
    /// </summary>
    public abstract class AttachmentAbility : StaticAbility
    {
        #region Variables

        private int[] m_attachedEffects;
        private static readonly Property<int[]> AttachedEffectsProperty = Property<int[]>.RegisterProperty<AttachmentAbility>("AttachedEffects", a => a.m_attachedEffects);

        #endregion

        #region Properties

        private IEnumerable<Object> AttachedEffects
        {
            get 
            {
                return m_attachedEffects == null ?
                    new Object[0] :
                    m_attachedEffects.Select(i => Manager.GetObjectByIdentifier<Object>(i));
            }
            set
            {
                SetValue(AttachedEffectsProperty, value == null ? null : value.Select(e => e.Identifier).ToArray(), ref m_attachedEffects);
            }
        }

        #endregion

        #region Methods

        private void Attach_Internal(Card card)
        {
            Debug.Assert(!AttachedEffects.Any());

            List<Object> attachedEffectInstances = new List<Object>();

            foreach (var creator in Attach(AddEffect.On(card)))
            {
                attachedEffectInstances.Add(creator.Create());
            }

            AttachedEffects = attachedEffectInstances;
        }

        private void Detach_Internal()
        {
            foreach (Object instance in AttachedEffects)
            {
                instance.Remove();
            }

            AttachedEffects = null;
        }

        protected abstract IEnumerable<IEffectCreator> Attach(ILocalEffectHost<Card> cardEffectHost);

        protected override void Init()
        {
            base.Init();

            Source.PropertyChanged += Source_PropertyChanged;
        }

        protected override void Uninit()
        {
            Source.PropertyChanged -= Source_PropertyChanged;

            base.Uninit();
        }

        #endregion

        #region Event Handlers

        void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.Property == Card.AttachedToProperty && Manager.IsMaster)
            {
                Detach_Internal();

                if (e.NewValue != null)
                {
                    Attach_Internal((Card)e.NewValue);
                }
            }
        }

        #endregion
    }
}
