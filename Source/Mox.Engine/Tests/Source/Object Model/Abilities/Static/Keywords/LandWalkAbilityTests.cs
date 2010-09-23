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
using System.Linq;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class LandWalkAbilityTests : BaseGameTests
    {
        #region Variables

        private BasicLandWalkAbility m_ability;
        private Card m_defender;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_ability = m_game.CreateAbility<BasicLandWalkAbility>(m_card);
            m_ability.Type = SubType.Island;
            m_defender = CreateCard(m_playerB);
        }

        #endregion

        #region Utilities

        private bool CanBlock()
        {
            return m_ability.CanBlock(m_card, m_defender);
        }

        private Card CreateLand(params SubType[] landType)
        {
            Card land = CreateCard(m_playerB);
            land.Type = Type.Land;
            land.SubTypes = new SubTypes(landType);
            land.Zone = m_game.Zones.Battlefield;
            return land;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_block_when_defender_has_no_land()
        {
            Assert.Collections.IsEmpty(m_playerB.Battlefield.Where(c => c.Is(Type.Land)));

            Assert.IsTrue(CanBlock());
        }

        [Test]
        public void Test_Can_block_when_defender_has_non_affected_land()
        {
            CreateLand(SubType.Forest, SubType.Mountain);
            CreateLand(SubType.Swamp);

            Assert.IsTrue(CanBlock());
        }

        [Test]
        public void Test_Cannot_block_when_defender_has_at_least_one_affected_land()
        {
            CreateLand(SubType.Forest, SubType.Mountain);
            CreateLand(SubType.Swamp);
            CreateLand(SubType.Plains, SubType.Island);

            Assert.IsFalse(CanBlock());
        }

        #endregion
    }
}
