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
    public class FactoryInstantsRedTests : BaseFactoryTests
    {
        #region Shock

        [Test]
        public void Test_Shock_player()
        {
            Card shock = InitializeCard("Shock");
            Assert.AreEqual(Type.Instant, shock.Type);
            
            Assert.AreEqual(1, shock.Abilities.Count());
            PlayCardAbility playAbility = GetPlayCardAbility(shock);

            m_playerB.Life = 20;

            Assert.IsTrue(CanPlay(m_playerA, playAbility));

            Expect_Target(m_playerA, m_playerB);
            Expect_PayManaCost(m_playerA, "R");
            PlayAndResolve(m_playerA, playAbility);

            Assert.AreEqual(m_game.Zones.Graveyard, shock.Zone);
            Assert.AreEqual(18, m_playerB.Life);
        }

        [Test]
        public void Test_Can_shock_a_creature_and_kill_it()
        {
            Card shock = InitializeCard("Shock");
            Assert.AreEqual(Type.Instant, shock.Type);

            Card creature = InitializeCard("Dross Crocodile");
            creature.Zone = m_game.Zones.Battlefield;

            Assert.AreEqual(1, shock.Abilities.Count());
            PlayCardAbility playAbility = GetPlayCardAbility(shock);

            m_playerB.Life = 20;

            Assert.IsTrue(CanPlay(m_playerA, playAbility));

            Expect_Target(m_playerA, creature);
            Expect_PayManaCost(m_playerA, "R");
            PlayAndResolve(m_playerA, playAbility);
            Assert.AreEqual(m_game.Zones.Graveyard, shock.Zone);

            Assert.AreEqual(2, creature.Damage);
        }

        #endregion
    }
}
