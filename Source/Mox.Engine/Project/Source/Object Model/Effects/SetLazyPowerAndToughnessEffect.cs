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
using System.Linq;

namespace Mox.Effects
{
    [Serializable]
    public abstract class LazyEffect<TValue> : MTGEffect<TValue>
    {
        private readonly List<PropertyIdentifier> m_invalidateProperties;

        protected LazyEffect(Property<TValue> property, params PropertyBase[] invalidateProperties)
            : base(property)
        {
            m_invalidateProperties = new List<PropertyIdentifier>(invalidateProperties.Length);
            foreach (var invalidateProperty in invalidateProperties)
            {
                m_invalidateProperties.Add(new PropertyIdentifier(invalidateProperty));
            }
        }

        protected override bool Invalidate(PropertyBase property)
        {
            if (property == Card.ZoneIdProperty || property == Card.ControllerProperty)
            {
                return true;
            }

            if (m_invalidateProperties.Any(pi => pi.Property == property))
            {
                return true;
            }

            return false;
        }
    }

    [Serializable]
    public class SetLazyPowerAndToughnessEffect : LazyEffect<PowerAndToughness>
    {
        #region Variables

        private readonly Func<Object, PowerAndToughness> m_provider;

        #endregion

        #region Constructor

        public SetLazyPowerAndToughnessEffect(Func<Object, PowerAndToughness> provider, params PropertyBase[] invalidateProperties)
            : base(Card.PowerAndToughnessProperty, invalidateProperties)
        {
            Throw.IfNull(provider, "provider");

            m_provider = provider;
        }

        #endregion

        #region Properties

        public override EffectDependencyLayer DependendencyLayer
        {
            get { return EffectDependencyLayer.PT_Set; }
        }

        #endregion

        #region Methods

        public override PowerAndToughness Modify(Object owner, PowerAndToughness value)
        {
            return m_provider(owner);
        }

        #endregion
    }
}
