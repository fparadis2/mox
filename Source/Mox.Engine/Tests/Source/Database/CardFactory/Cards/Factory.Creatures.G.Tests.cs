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
    public class FactoryCreatureGreenTests : BaseFactoryTests
    {
        #region Dryad Arbor

        [Test]
        public void Test_Dryad_Arbor()
        {
            Card card = InitializeCard("Dryad Arbor");
            Assert.AreEqual(Type.Land | Type.Creature, card.Type);

            Assert.AreEqual(2, card.Abilities.Count());
            var playCardAbility = card.Abilities.OfType<PlayCardAbility>().Single();

            Assert.IsTrue(CanPlay(m_playerA, playCardAbility));
            Play(m_playerA, playCardAbility);
            Assert.AreEqual(m_game.Zones.Battlefield, card.Zone);

            var tapForMana = card.Abilities.OfType<ActivatedAbility>().Single();
            Assert.IsTrue(CanPlay(m_playerA, tapForMana));
            Play(m_playerA, tapForMana);
            Assert.AreEqual(1, m_playerA.ManaPool.Green);
        }

        #endregion
    }
}
