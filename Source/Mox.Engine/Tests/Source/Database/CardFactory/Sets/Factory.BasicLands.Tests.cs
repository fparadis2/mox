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

namespace Mox.Database.Sets
{
    [TestFixture]
    public class FactoryBasicLandsTests : BaseFactoryTests
    {
        #region Utilities

        private void Test_BasicLand(string cardName, Color color)
        {
            Card basicLand = CreateCard<BasicLandCardFactory>(m_playerA, "10E", cardName);
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

        #endregion
    }
}
