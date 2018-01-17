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
    public class FactoryCreatureWhiteTests : BaseFactoryTests
    {
        #region Loxodon Mystic

        [Test]
        public void Test_Loxodon_Mystic_can_tap_a_card()
        {
            Card creatureCard = InitializeCard("Loxodon Mystic");
            Card otherCard = InitializeCard("Savannah Lions");
            otherCard.Zone = m_game.Zones.Battlefield;

            Assert.That(!otherCard.Tapped);

            var playCardAbility = GetPlayCardAbility(creatureCard);

            Assert.IsTrue(CanPlay(m_playerA, playCardAbility));

            Expect_PayManaCost(m_playerA, "3WW");
            PlayAndResolve(m_playerA, playCardAbility);

            Assert.AreEqual(m_game.Zones.Battlefield, creatureCard.Zone);

            ActivatedAbility tapAbility = creatureCard.Abilities.OfType<ActivatedAbility>().First();
            Assert.IsTrue(CanPlay(m_playerA, tapAbility));

            Expect_Target(m_playerA, TargetCost.Creature(), otherCard);
            Expect_PayManaCost(m_playerA, "W");
            PlayAndResolve(m_playerA, tapAbility);

            Assert.That(otherCard.Tapped);
        }

        [Test]
        public void Test_Loxodon_Mystic_can_tap_itself()
        {
            Card creatureCard = InitializeCard("Loxodon Mystic");

            var playCardAbility = GetPlayCardAbility(creatureCard);

            Assert.IsTrue(CanPlay(m_playerA, playCardAbility));

            Expect_PayManaCost(m_playerA, "3WW");
            PlayAndResolve(m_playerA, playCardAbility);

            Assert.AreEqual(m_game.Zones.Battlefield, creatureCard.Zone);

            ActivatedAbility tapAbility = creatureCard.Abilities.OfType<ActivatedAbility>().First();
            Assert.IsTrue(CanPlay(m_playerA, tapAbility));

            Expect_Target(m_playerA, TargetCost.Creature(), creatureCard);
            Expect_PayManaCost(m_playerA, "W");
            PlayAndResolve(m_playerA, tapAbility);

            Assert.That(creatureCard.Tapped);
        }

        [Test]
        public void Test_Loxodon_Mystic_wont_do_anything_if_its_target_becomes_tapped()
        {
            Card creatureCard = InitializeCard("Loxodon Mystic");
            Card anotherCreatureCard = InitializeCard("Loxodon Mystic");

            creatureCard.Zone = m_game.Zones.Battlefield;
            anotherCreatureCard.Zone = m_game.Zones.Battlefield;

            var tapAbility = creatureCard.Abilities.OfType<ActivatedAbility>().First();
            Assert.IsTrue(CanPlay(m_playerA, tapAbility));

            Expect_Target(m_playerA, TargetCost.Creature(), anotherCreatureCard);
            Expect_PayManaCost(m_playerA, "W");

            Play(m_playerA, tapAbility);

            Assert.That(!anotherCreatureCard.Tapped);
            anotherCreatureCard.Tap();
            Assert.That(anotherCreatureCard.Tapped);

            Expect_AllPlayersPass(m_playerA);
            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            Assert.That(anotherCreatureCard.Tapped);
        }

        #endregion
    }
}
