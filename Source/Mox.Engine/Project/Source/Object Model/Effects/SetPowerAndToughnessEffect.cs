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

namespace Mox.Effects
{
    [Serializable]
    public class SetPowerAndToughnessEffect : MTGEffect<PowerAndToughness>
    {
        #region Variables

        private readonly int m_power;
        private readonly int m_toughness;

        #endregion

        #region Constructor

        public SetPowerAndToughnessEffect(int power, int toughness)
            : base(Card.PowerAndToughnessProperty)
        {
            m_power = power;
            m_toughness = toughness;
        }

        #endregion

        #region Properties

        public override EffectDependencyLayer DependendencyLayer
        {
            get { return EffectDependencyLayer.PT_Set; }
        }

        public int Power
        {
            get { return m_power; }
        }

        public int Toughness
        {
            get { return m_toughness; }
        }

        #endregion

        #region Methods

        public override PowerAndToughness Modify(Object owner, PowerAndToughness value)
        {
            value.Power = Power;
            value.Toughness = Toughness;

            return value;
        }

        #endregion
    }
}
