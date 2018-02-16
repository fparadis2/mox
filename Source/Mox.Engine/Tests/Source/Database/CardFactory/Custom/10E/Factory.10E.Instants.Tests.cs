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

using Mox.Abilities;

namespace Mox.Database.Sets
{
#warning todo spell_v2
    /*
    [TestFixture]
    public class Factory10EInstantsTests : BaseFactoryTests
    {
        #region Utilities

        private Card CreateCreature(int power, int toughness)
        {
            Card creature = CreateCard(m_playerA);
            creature.Type = Type.Creature;
            creature.Power = power;
            creature.Toughness = toughness;
            creature.Zone = m_game.Zones.Battlefield;
            return creature;
        }

        private void FillLibrary(Player player)
        {
            for (int i = 0; i < 10; i++)
            {
                CreateCard(player).Zone = m_game.Zones.Library;
            }
        }

        #endregion

        #region Tests

        #region White

        #region Condemn

        [Test]
        public void Test_Condemn()
        {
            Card afflict = InitializeCard("Condemn");
            Assert.AreEqual(Type.Instant, afflict.Type);

            Assert.AreEqual(1, afflict.Abilities.Count());
            Assert.IsInstanceOf<PlayCardAbility>(afflict.Abilities.Single());

            Card vanillaCreature = CreateCreature(2, 4);
            vanillaCreature.Controller = m_playerB;
            m_game.CombatData.Attackers = new DeclareAttackersResult(vanillaCreature);

            Assert.IsTrue(CanPlay(m_playerA, afflict.Abilities.Single()));
            using (m_mockery.Ordered())
            {
                Expect_Target(m_playerA, TargetCost.Creature().Attacking(), vanillaCreature);
                Expect_PayManaCost(m_playerA, "W");
            }

            Assert.AreEqual(20, m_playerB.Life);

            PlayAndResolve(m_playerA, afflict.Abilities.Single());

            Assert.AreEqual(vanillaCreature, m_playerA.Library.First());
            Assert.AreEqual(24, m_playerB.Life);
        }

        #endregion

        #endregion

        #region Red

        #region Beacon of Destruction

        [Test]
        public void Test_Beacon_of_Destruction()
        {
            FillLibrary(m_playerA);

            Card card = InitializeCard("Beacon of Destruction");
            Assert.AreEqual(Type.Instant, card.Type);

            Assert.AreEqual(1, card.Abilities.Count());
            PlayCardAbility ability = card.Abilities.OfType<PlayCardAbility>().Single();

            m_playerB.Life = 20;

            Assert.IsTrue(CanPlay(m_playerA, ability));
            using (m_mockery.Ordered())
            {
                Expect_Target(m_playerA, TargetCost.Player() | TargetCost.Creature(), m_playerB);
                Expect_PayManaCost(m_playerA, "3RR");
            }
            PlayAndResolve(m_playerA, ability);

            Assert.AreEqual(m_game.Zones.Library, card.Zone);
            Assert.AreEqual(15, m_playerB.Life);
        }

        #endregion

        #region Soulblast

        [Test]
        public void Test_Soulblast()
        {
            Card card1 = CreateCreature(3, 1);
            Card card2 = CreateCreature(4, 2);

            Card instant = InitializeCard("Soulblast");
            Assert.AreEqual(Type.Instant, instant.Type);

            PlayCardAbility playAbility = instant.Abilities.OfType<PlayCardAbility>().Single();

            m_playerB.Life = 20;

            Assert.IsTrue(CanPlay(m_playerA, playAbility));
            using (m_mockery.Ordered())
            {
                Expect_Target(m_playerA, TargetCost.Player() | TargetCost.Creature(), m_playerB);
                Expect_PayManaCost(m_playerA, "3RRR");
            }
            PlayAndResolve(m_playerA, playAbility);

            Assert.AreEqual(m_game.Zones.Graveyard, card1.Zone);
            Assert.AreEqual(m_game.Zones.Graveyard, card2.Zone);

            Assert.AreEqual(13, m_playerB.Life);
        }

        #endregion

        #endregion

        #endregion
    }*/
}
