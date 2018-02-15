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

namespace Mox.Abilities
{
    [Serializable]
    public class SetPowerAndToughnessEffect : LazyMTGEffect<PowerAndToughness>
    {
        #region Constructor

        public SetPowerAndToughnessEffect(ISpellContext spellContext, AmountResolver power, AmountResolver toughness)
            : base(Card.PowerAndToughnessProperty, spellContext)
        {
            Power = power;
            Toughness = toughness;
        }

        #endregion

        #region Properties

        public override EffectDependencyLayer DependendencyLayer
        {
            get { return EffectDependencyLayer.PT_Set; }
        }

        public AmountResolver Power { get; }
        public AmountResolver Toughness { get; }

        #endregion

        #region Methods

        public override PowerAndToughness Modify(Object owner, PowerAndToughness value)
        {
            var spellContext = GetSpellContext(owner);

            value.Power = Power.Resolve(spellContext);
            value.Toughness = Toughness.Resolve(spellContext);

            return value;
        }

        protected override bool Invalidate(PropertyBase property)
        {
            return
                Power.Invalidate(property) ||
                Toughness.Invalidate(property);
        }

        #endregion
    }
}
