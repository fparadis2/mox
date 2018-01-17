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
    public class FactoryCreatureBlackTests : BaseFactoryTests
    {
        #region Vanilla

        [Test]
        public void Test_Vanilla_creature()
        {
            Card creatureCard = InitializeCard("Mass of Ghouls");
            Assert.AreEqual(Type.Creature, creatureCard.Type);

            Assert.AreEqual(5, creatureCard.Power);
            Assert.AreEqual(3, creatureCard.Toughness);

            Assert.AreEqual(1, creatureCard.Abilities.Count());
            Assert.IsInstanceOf<PlayCardAbility>(creatureCard.Abilities.First());

            Assert.IsTrue(CanPlay(m_playerA, creatureCard.Abilities.First()));

            Expect_PayManaCost(m_playerA, "3BB");
            PlayAndResolve(m_playerA, GetPlayCardAbility(creatureCard));

            Assert.AreEqual(m_game.Zones.Battlefield, creatureCard.Zone);
        }

        #endregion
    }
}
