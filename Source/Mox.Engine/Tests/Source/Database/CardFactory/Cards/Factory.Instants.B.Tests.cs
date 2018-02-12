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
    public class FactoryInstantsBlackTests : BaseFactoryTests
    {
        #region Afflict

        [Test]
        public void Test_Afflict()
        {
            Card afflict = InitializeCard("Afflict");
            Assert.AreEqual(Type.Instant, afflict.Type);

            PlayCardAbility playAbility = GetPlayCardAbility(afflict);

            Card vanillaCreature = CreateCreatureOnBattlefield(2, 2);
            int numCards = m_playerA.Hand.Count;

            Assert.IsTrue(CanPlay(m_playerA, playAbility));
            Expect_Target(m_playerA, PermanentFilter.AnyCreature, vanillaCreature);
            Expect_PayManaCost(m_playerA, "2B");
            PlayAndResolve(m_playerA, playAbility);

            Assert.AreEqual(1, vanillaCreature.Power);
            Assert.AreEqual(1, vanillaCreature.Toughness);
            Assert.AreEqual(numCards, m_playerA.Hand.Count);

            m_game.Events.Trigger(new EndOfTurnEvent(m_playerA));

            Assert.AreEqual(2, vanillaCreature.Power);
            Assert.AreEqual(2, vanillaCreature.Toughness);
        }

        #endregion
    }
}
