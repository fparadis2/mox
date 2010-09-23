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
using System.Text;

namespace Mox.Effects
{
    [Serializable]
    public class ModifyPowerAndToughnessEffect : MTGEffect<PowerAndToughness>
    {
        #region Variables

        private readonly int m_powerModifier;
        private readonly int m_toughnessModifier;

        #endregion

        #region Constructor

        public ModifyPowerAndToughnessEffect(int powerModifier, int toughnessModifier)
            : base(Card.PowerAndToughnessProperty)
        {
            m_powerModifier = powerModifier;
            m_toughnessModifier = toughnessModifier;
        }

        #endregion

        #region Properties

        public override EffectDependencyLayer DependendencyLayer
        {
            get { return EffectDependencyLayer.PT_Modify; }
        }

        public int PowerModifier
        {
            get { return m_powerModifier; }
        }

        public int ToughnessModifier
        {
            get { return m_toughnessModifier; }
        }

        #endregion

        #region Methods

        public override PowerAndToughness Modify(Object owner, PowerAndToughness value)
        {
            value.Power += PowerModifier;
            value.Toughness += ToughnessModifier;

            return value;
        }

        #endregion
    }
}
