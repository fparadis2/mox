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
using Mox.Rules;
using NUnit.Framework;
using Mox.Abilities;

namespace Mox.Database.Sets
{
    [TestFixture]
    public class FactoryEnchantmentsWhiteTests : BaseFactoryTests
    {
        #region Glorious Anthem

        [Test]
        public void Test_Glorious_Anthem()
        {
            Card card = InitializeCard("Glorious Anthem");
            card.Zone = m_game.Zones.Battlefield;

            Card creature1 = CreateCreatureOnBattlefield(1, 1);
            Card creature2 = CreateCreatureOnBattlefield(1, 1);

            Assert_PT(creature1, 2, 2);
            Assert_PT(creature2, 2, 2);

            creature1.Controller = m_playerB;
            Assert_PT(creature1, 1, 1);
            Assert_PT(creature2, 2, 2);

            creature1.Controller = m_playerA;
            Assert_PT(creature1, 2, 2);
            Assert_PT(creature2, 2, 2);

            creature1.Type = Type.Artifact;
            creature2.Type = Type.Creature | Type.Land;
            Assert_PT(creature1, 1, 1);
            Assert_PT(creature2, 2, 2);
        }

        [Test]
        public void Test_Glorious_Anthem_entering_and_leaving_play()
        {
            Card card = InitializeCard("Glorious Anthem");
            Card creature1 = CreateCreatureOnBattlefield(1, 1);
            Card creature2 = CreateCreatureOnBattlefield(1, 1);

            Assert_PT(creature1, 1, 1);
            Assert_PT(creature2, 1, 1);

            card.Zone = m_game.Zones.Battlefield;

            Assert_PT(creature1, 2, 2);
            Assert_PT(creature2, 2, 2);

            card.Zone = m_game.Zones.Hand;

            Assert_PT(creature1, 1, 1);
            Assert_PT(creature2, 1, 1);
        }

        [Test]
        public void Test_Glorious_Anthem_changing_controller()
        {
            Card card = InitializeCard("Glorious Anthem");
            card.Zone = m_game.Zones.Battlefield;

            Card creature1 = CreateCreatureOnBattlefield(m_playerA, 1, 1);
            Card creature2 = CreateCreatureOnBattlefield(m_playerB, 1, 1);

            Assert_PT(creature1, 2, 2);
            Assert_PT(creature2, 1, 1);

            card.Controller = m_playerB;

            Assert_PT(creature1, 1, 1);
            Assert_PT(creature2, 2, 2);
        }

        #endregion

        #region Holy Strength

        [Test]
        public void Test_Holy_Strength()
        {
            Card creature1 = CreateCreatureOnBattlefield(1, 1);
            Card creature2 = CreateCreatureOnBattlefield(1, 1);

            Card card = InitializeCard("Holy Strength");
            Expect_Target(m_playerA, creature1);
            Expect_PayManaCost(m_playerA, "W");
            PlayAndResolve(m_playerA, GetPlayCardAbility(card));

            Assert_PT(creature1, 2, 3);
            Assert_PT(creature2, 1, 1);

            creature1.Controller = m_playerB;

            Assert_PT(creature1, 2, 3);
            Assert_PT(creature2, 1, 1);

            card.Zone = m_game.Zones.Hand;

            Assert_PT(creature1, 1, 1);
            Assert_PT(creature2, 1, 1);
        }

        #endregion
    }
}
