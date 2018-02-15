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

namespace Mox
{
    [Serializable]
    internal abstract class CompositeCondition : Condition
    {
        #region Variables

        protected readonly Condition m_conditionA;
        protected readonly Condition m_conditionB;

        #endregion

        #region Constructor

        protected CompositeCondition(Condition conditionA, Condition conditionB)
        {
            Throw.IfNull(conditionA, "conditionA");
            Throw.IfNull(conditionB, "conditionB");

            m_conditionA = conditionA;
            m_conditionB = conditionB;
        }

        #endregion

        #region Methods

        protected internal override bool Invalidate(PropertyBase property)
        {
            return m_conditionA.Invalidate(property) || m_conditionB.Invalidate(property);
        }

        #endregion
    }

    [Serializable]
    internal sealed class AndCondition : CompositeCondition
    {
        public AndCondition(Condition conditionA, Condition conditionB)
            : base(conditionA, conditionB)
        {
        }

        public override bool Matches(Card card)
        {
            return m_conditionA.Matches(card) && m_conditionB.Matches(card);
        }
    }
}
