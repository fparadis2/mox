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
using Mox.Database.Library;

namespace Mox.Database.Sets
{
    [TestFixture]
    public class FactoryCreatureGreenTests : BaseFactoryTests
    {
        #region Utilities

        private void Test_BasicLand(string cardName, Color color)
        {
            Card basicLand = InitializeCard(cardName);
            Assert.AreEqual(Type.Land, basicLand.Type);

            var abilities = basicLand.Abilities.ToList();

            Assert.AreEqual(2, abilities.Count);
            Assert.IsInstanceOf<PlayCardAbility>(abilities[0]);

            Assert.IsTrue(CanPlay(m_playerA, abilities[0]));
            Assert.IsFalse(CanPlay(m_playerA, abilities[1]));
            Play(m_playerA, abilities[0]);
            Assert.AreEqual(m_game.Zones.Battlefield, basicLand.Zone);

            Assert.IsFalse(CanPlay(m_playerA, abilities[0]));
            Assert.IsTrue(CanPlay(m_playerA, abilities[1]));
            Assert.AreEqual(AbilityType.Normal, abilities[1].AbilityType);
            Assert.IsTrue(abilities[1].IsManaAbility);

            Assert.AreEqual(0, m_playerA.ManaPool[color], "Sanity check");
            Play(m_playerA, abilities[1]);
            Assert.IsTrue(basicLand.Tapped);
            Assert.AreEqual(1, m_playerA.ManaPool[color]);

            // Cannot play when tapped
            Assert.IsFalse(CanPlay(m_playerA, abilities[1]));
        }

        private void Test_DualLand(string cardName, params Color[] colors)
        {
            m_playerA.ManaPool.Clear();

            Card land = InitializeCard(cardName);
            Assert.AreEqual(Type.Land, land.Type);

            var abilities = land.Abilities.ToList();

            Assert.AreEqual(1 + colors.Length, abilities.Count);
            Assert.IsInstanceOf<PlayCardAbility>(abilities[0]);

            Assert.IsTrue(CanPlay(m_playerA, abilities[0]));            
            for (int i = 1; i < abilities.Count; i++)
            {
                Assert.IsFalse(CanPlay(m_playerA, abilities[i]));
            }

            Play(m_playerA, abilities[0]);
            Assert.AreEqual(m_game.Zones.Battlefield, land.Zone);
            
            Assert.IsFalse(CanPlay(m_playerA, abilities[0]));
            for (int i = 1; i < abilities.Count; i++)
            {
                land.Tapped = false;

                Assert.IsTrue(CanPlay(m_playerA, abilities[i]));
                Assert.AreEqual(AbilityType.Normal, abilities[i].AbilityType);
                Assert.IsTrue(abilities[i].IsManaAbility);
                
                Play(m_playerA, abilities[i]);
                Assert.IsTrue(land.Tapped);
            }

            foreach (var color in colors)
            {
                Assert.AreEqual(1, m_playerA.ManaPool[color]);
            }

            // Cannot play when tapped
            land.Tapped = true;
            for (int i = 1; i < abilities.Count; i++)
            {
                Assert.IsFalse(CanPlay(m_playerA, abilities[i]));
            }
        }

        #endregion

        #region Tests

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

            var tapForMana = card.Abilities.OfType<TapForManaAbility>().Single();
            Assert.IsTrue(CanPlay(m_playerA, tapForMana));
            Play(m_playerA, tapForMana);
            Assert.AreEqual(1, m_playerA.ManaPool.Green);
        }

        #endregion
    }
}
