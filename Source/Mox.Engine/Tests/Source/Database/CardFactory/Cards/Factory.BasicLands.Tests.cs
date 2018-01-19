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
    public class FactoryBasicLandsTests : BaseFactoryTests
    {
        #region Utilities

        private void Test_BasicLand(string cardName, Color color)
        {
            Card basicLand = InitializeCard(cardName);
            Assert.AreEqual(Type.Land, basicLand.Type);

            var abilities = basicLand.Abilities.ToList();
            Assert.AreEqual(2, abilities.Count);

            var playCardAbility = abilities.OfType<PlayCardAbility>().Single();
            var tapAbility = abilities.OfType<ActivatedAbility>().Single();

            Assert.IsTrue(CanPlay(m_playerA, playCardAbility));
            Assert.IsFalse(CanPlay(m_playerA, tapAbility));
            Play(m_playerA, playCardAbility);
            Assert.AreEqual(m_game.Zones.Battlefield, basicLand.Zone);

            Assert.IsFalse(CanPlay(m_playerA, playCardAbility));
            Assert.IsTrue(CanPlay(m_playerA, tapAbility));
            Assert.AreEqual(AbilityType.Normal, tapAbility.AbilityType);
            Assert.IsTrue(tapAbility.IsManaAbility);

            Assert.AreEqual(0, m_playerA.ManaPool[color], "Sanity check");
            Play(m_playerA, tapAbility);
            Assert.IsTrue(basicLand.Tapped);
            Assert.AreEqual(1, m_playerA.ManaPool[color]);

            // Cannot play when tapped
            Assert.IsFalse(CanPlay(m_playerA, tapAbility));
        }

        private void Test_DualLand(string cardName, params Color[] colors)
        {
            m_playerA.ManaPool.Clear();

            Card land = InitializeCard(cardName);
            Assert.AreEqual(Type.Land, land.Type);

            var activatedAbilities = land.Abilities.OfType<ActivatedAbility>().ToList();
            var playCardAbility = land.Abilities.OfType<PlayCardAbility>().Single();
            Assert.AreEqual(colors.Length, activatedAbilities.Count);

            Assert.IsTrue(CanPlay(m_playerA, playCardAbility));
            foreach (var tapAbility in activatedAbilities)
            {
                Assert.IsFalse(CanPlay(m_playerA, tapAbility));
            }

            Play(m_playerA, playCardAbility);
            Assert.AreEqual(m_game.Zones.Battlefield, land.Zone);
            
            Assert.IsFalse(CanPlay(m_playerA, playCardAbility));
            foreach (var tapAbility in activatedAbilities)
            {
                land.Tapped = false;

                Assert.IsTrue(CanPlay(m_playerA, tapAbility));
                Assert.AreEqual(AbilityType.Normal, tapAbility.AbilityType);
                Assert.IsTrue(tapAbility.IsManaAbility);
                
                Play(m_playerA, tapAbility);
                Assert.IsTrue(land.Tapped);
            }

            foreach (var color in colors)
            {
                Assert.AreEqual(1, m_playerA.ManaPool[color]);
            }

            // Cannot play when tapped
            land.Tapped = true;
            foreach (var tapAbility in activatedAbilities)
            {
                Assert.IsFalse(CanPlay(m_playerA, tapAbility));
            }
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Basic_lands_in_play()
        {
            using (OneLandPerTurn.Bypass())
            {
                Test_BasicLand("Forest", Color.Green);
                Test_BasicLand("Swamp", Color.Black);
                Test_BasicLand("Plains", Color.White);
                Test_BasicLand("Mountain", Color.Red);
                Test_BasicLand("Island", Color.Blue);
            }
        }

        [Test]
        public void Test_Dual_lands_in_play()
        {
            using (OneLandPerTurn.Bypass())
            {
                Test_DualLand("Tundra", Color.White, Color.Blue);
                Test_DualLand("Underground Sea", Color.Blue, Color.Black);
                Test_DualLand("Badlands", Color.Black, Color.Red);
                Test_DualLand("Taiga", Color.Red, Color.Green);
                Test_DualLand("Savannah", Color.Green, Color.White);

                Test_DualLand("Scrubland", Color.White, Color.Black);
                Test_DualLand("Volcanic Island", Color.Blue, Color.Red);
                Test_DualLand("Bayou", Color.Black, Color.Green);
                Test_DualLand("Plateau", Color.Red, Color.White);
                Test_DualLand("Tropical Island", Color.Green, Color.Blue);
            }
        }

        #endregion
    }
}
