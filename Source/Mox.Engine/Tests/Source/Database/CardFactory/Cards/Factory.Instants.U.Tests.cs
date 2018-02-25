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
using Mox.Events;

namespace Mox.Database.Sets
{
    [TestFixture]
    public class FactoryInstantsBlueTests : BaseFactoryTests
    {
        #region Counterspell

        [Test]
        public void Test_Counterspell()
        {
            Card counterspell = InitializeCard("Counterspell");
            Assert.AreEqual(Type.Instant, counterspell.Type);

            PlayCardAbility playAbility = GetPlayCardAbility(counterspell);

            Card creatureCard = InitializeCard("Ornithopter");
            Play(m_playerA, GetPlayCardAbility(creatureCard));

            Assert.AreEqual(1, m_game.SpellStack2.Count);

            Expect_Target(m_playerA, StackFilter.AnySpell, m_game.SpellStack2.Last());
            Expect_PayManaCost(m_playerA, "UU");
            PlayAndResolve(m_playerA, playAbility);

            Assert.AreEqual(0, m_game.SpellStack2.Count);
            Assert.AreEqual(m_game.Zones.Graveyard, creatureCard.Zone);
        }

        [Test]
        public void Test_Counterspell_cannot_counter_activated_abilities()
        {
            Card counterspell = InitializeCard("Counterspell");
            Assert.AreEqual(Type.Instant, counterspell.Type);

            PlayCardAbility playAbility = GetPlayCardAbility(counterspell);

            Card creatureCard = InitializeCard("Prodigal Sorcerer");
            creatureCard.Zone = m_game.Zones.Battlefield;

            Expect_Target(m_playerA, m_playerA);
            Play(m_playerA, creatureCard.Abilities.OfType<ActivatedAbility>().Single());

            var prodigalSpell = m_game.SpellStack2.Single();

            Expect_Target(m_playerA, StackFilter.AnySpell, m_game.SpellStack2.Last());
            Expect_Target(m_playerA, StackFilter.AnySpell, null);
            Play(m_playerA, playAbility);

            Assert.Collections.AreEqual(new[] { prodigalSpell }, m_game.SpellStack2);
        }

        #endregion
    }
}
