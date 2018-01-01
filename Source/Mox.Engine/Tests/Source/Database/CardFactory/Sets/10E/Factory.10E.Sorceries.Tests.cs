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
using Mox.Events;
using NUnit.Framework;

namespace Mox.Database.Sets
{
    [TestFixture]
    public class Factory10ESorceriesTests : BaseFactoryTests
    {
        #region Utilities

        private Card CreateCreature(Player owner, int power, int toughness)
        {
            Card creature = CreateCard(owner);
            creature.Type = Type.Creature;
            creature.Power = power;
            creature.Toughness = toughness;
            creature.Zone = m_game.Zones.Battlefield;
            return creature;
        }

        #endregion

        #region Tests

        #region Sorceries

        #region Red

        #region Spitting Earth

        [Test]
        public void Test_Spitting_Earth()
        {
            Card afflict = InitializeCard("Spitting Earth");
            Assert.AreEqual(Type.Sorcery, afflict.Type);

            InitializeCard("Mountain").Zone = m_game.Zones.Battlefield;
            InitializeCard("Mountain").Zone = m_game.Zones.Battlefield;
            InitializeCard("Mountain").Zone = m_game.Zones.Battlefield;

            Assert.AreEqual(1, afflict.Abilities.Count());
            PlayCardAbility ability = afflict.Abilities.OfType<PlayCardAbility>().Single();

            Card vanillaCreature = CreateCreature(m_playerA, 4, 4);

            Assert.IsTrue(CanPlay(m_playerA, ability));
            using (m_mockery.Ordered())
            {
                Expect_Target(m_playerA, TargetCost.Creature(), vanillaCreature);
                Expect_PayManaCost(m_playerA, "1R");
            }
            PlayAndResolve(m_playerA, ability);

            Assert.AreEqual(3, vanillaCreature.Damage);
        }

        #endregion

        #region Threaten

        [Test]
        public void Test_Threaten()
        {
            Card sorcery = InitializeCard("Threaten");
            Assert.AreEqual(Type.Sorcery, sorcery.Type);

            Assert.AreEqual(1, sorcery.Abilities.Count());
            PlayCardAbility ability = sorcery.Abilities.OfType<PlayCardAbility>().Single();

            Card vanillaCreature = CreateCreature(m_playerB, 4, 4);

            Assert.IsTrue(CanPlay(m_playerA, ability));
            using (m_mockery.Ordered())
            {
                Expect_Target(m_playerA, TargetCost.Creature(), vanillaCreature);
                Expect_PayManaCost(m_playerA, "2R");
            }
            PlayAndResolve(m_playerA, ability);

            Assert.AreEqual(m_playerA, vanillaCreature.Controller);
            Assert.IsFalse(vanillaCreature.Tapped);
            Assert.IsTrue(vanillaCreature.HasAbility<HasteAbility>());

            m_game.Events.Trigger(new EndOfTurnEvent(m_playerA));

            Assert.AreEqual(m_playerB, vanillaCreature.Controller);
            Assert.IsFalse(vanillaCreature.Tapped);
            Assert.IsFalse(vanillaCreature.HasAbility<HasteAbility>());
        }

        #endregion

        #endregion

        #endregion

        #endregion
    }
}
