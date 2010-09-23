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
    public class Factory10ELandsTests : BaseFactoryTests
    {
        #region Utilities

        private void Test_Pain_land(string name, Color mana1, Color mana2)
        {
            Card card = CreateCard<PainLandCardFactory>(m_playerA, "10E", name);

            Assert.AreEqual(Type.Land, card.Type);

            var abilities = card.Abilities.ToList();

            Assert.AreEqual(4, abilities.Count);
            Assert.IsInstanceOf<PlayCardAbility>(abilities[0]);

            // Play from hand
            Assert.IsTrue(CanPlay(m_playerA, abilities[0]), "Should be able to play {0} from hand", name);
            Assert.IsFalse(CanPlay(m_playerA, abilities[1]));
            Assert.IsFalse(CanPlay(m_playerA, abilities[2]));
            Assert.IsFalse(CanPlay(m_playerA, abilities[3]));
            Play(m_playerA, abilities[0]);
            Assert.AreEqual(m_game.Zones.Battlefield, card.Zone);

            Test_Can_tap_for_mana(abilities[1], Color.None);
            
            using (Expect_Lose_One_Life(m_playerA))
            {
                Test_Can_tap_for_mana(abilities[2], mana1);
            }

            using (Expect_Lose_One_Life(m_playerA))
            {
                Test_Can_tap_for_mana(abilities[3], mana2);
            }
        }

        private static IDisposable Expect_Lose_One_Life(Player player)
        {
            int oldLife = player.Life;

            return new DisposableHelper(() => Assert.AreEqual(oldLife - 1, player.Life));
        }

        private void Test_Can_tap_for_mana(Ability ability, Color mana)
        {
            Player player = ability.Source.Controller;

            Assert.IsTrue(CanPlay(player, ability));
            Assert.AreEqual(AbilityType.Normal, ability.AbilityType);
            Assert.IsTrue(ability.IsManaAbility);

            int oldMana = player.ManaPool[mana];
            Play(m_playerA, ability);
            Assert.IsTrue(ability.Source.Tapped);
            Assert.AreEqual(oldMana + 1, player.ManaPool[mana], "Expected to get 1 mana of type {0}", mana);

            Assert.IsFalse(CanPlay(player, ability));

            // Reset
            ability.Source.Tapped = false;
        }

        #endregion

        #region Tests

        #region Pain Lands

        [Test]
        public void Test_Pain_lands()
        {
            using (OneLandPerTurn.Bypass())
            {
                Test_Pain_land("Adarkar Wastes", Color.White, Color.Blue);
                Test_Pain_land("Battlefield Forge", Color.Red, Color.White);
                Test_Pain_land("Brushland", Color.Green, Color.White);
                Test_Pain_land("Caves of Koilos", Color.White, Color.Black);
                Test_Pain_land("Karplusan Forest", Color.Red, Color.Green);
                Test_Pain_land("Llanowar Wastes", Color.Black, Color.Green);
                Test_Pain_land("Shivan Reef", Color.Blue, Color.Red);
                Test_Pain_land("Sulfurous Springs", Color.Black, Color.Red);
                Test_Pain_land("Underground River", Color.Blue, Color.Black);
                Test_Pain_land("Yavimaya Coast", Color.Green, Color.Blue);
            }
        }

        #endregion

        #endregion
    }
}
