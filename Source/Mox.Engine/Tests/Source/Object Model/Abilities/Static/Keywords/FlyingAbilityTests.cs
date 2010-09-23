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
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class FlyingAbilityTests : BaseGameTests
    {
        #region Variables

        private FlyingAbility m_ability;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_ability = m_game.CreateAbility<FlyingAbility>(m_card);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_only_block_flying_creatures_with_flying()
        {
            Card non_flying_creature = CreateCard(m_playerA);

            Assert.IsTrue(m_ability.CanBlock(m_card, m_card));
            Assert.IsFalse(m_ability.CanBlock(m_card, non_flying_creature));
        }

        [Test]
        public void Test_Creatures_with_reach_can_block_flying_creatures()
        {
            Card reachCreature = CreateCard(m_playerA);
            m_game.CreateAbility<ReachAbility>(reachCreature);

            Assert.IsTrue(m_ability.CanBlock(m_card, reachCreature));
        }

        #endregion
    }
}
