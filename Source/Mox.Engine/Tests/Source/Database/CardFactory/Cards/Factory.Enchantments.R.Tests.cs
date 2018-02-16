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
    public class FactoryEnchantmentsRedTests : BaseFactoryTests
    {
        #region Firebreathing

        [Test]
        public void Test_Firebreathing()
        {
            Card creature = CreateCreatureOnBattlefield(m_playerB, 1, 1);

            Card card = InitializeCard("Firebreathing");

            Expect_Target(m_playerA, creature);
            Expect_PayManaCost(m_playerA, "R");
            PlayAndResolve(m_playerA, card);

            Assert_PT(creature, 1, 1);

            var boostAbility = card.Abilities.OfType<ActivatedAbility>().Single();

            Assert.IsTrue(CanPlay(m_playerA, boostAbility));
            Expect_PayManaCost(m_playerA, "R");
            PlayAndResolve(m_playerA, boostAbility);

            Assert.AreEqual(2, creature.Power);
            Assert.AreEqual(1, creature.Toughness);

            card.Attach(null);

            // Effect is still present if aura is detached
            Assert.AreEqual(2, creature.Power);
            Assert.AreEqual(1, creature.Toughness);
        }

        #endregion
    }
}
