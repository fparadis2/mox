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
using NUnit.Framework;

using Mox.Abilities;

namespace Mox.Database
{
    [TestFixture]
    public class CardFactoryTests : BaseGameTests
    {
        #region Mock Types

        private class MockFactory : CardFactory
        { }

        #endregion

        #region Utility

        private Card CreateAndInitialize(string cardName, Player owner = null)
        {
            var card = CreateCard(owner ?? m_playerA, cardName);

            Assert.That(MasterCardDatabase.Instance.Cards.TryGetValue(cardName, out CardInfo cardInfo));
            Assert.IsNotNull(cardInfo);

            var factory = new MockFactory { CardInfo = cardInfo };

            var result = factory.InitializeCard(card);
            Assert.That(result.Type == CardFactoryResult.ResultType.Success);

            return card;
        }

        private void Assert_Color_is(string cardName, Color color)
        {
            var card = CreateAndInitialize(cardName);
            Assert.AreEqual(color, card.Color);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_InitializeCard_uses_the_master_database_to_initialize_the_card()
        {
            var card = CreateAndInitialize("Plains");

            Assert.AreEqual(Type.Land, card.Type);
            Assert.Collections.AreEqual(new[] { SubType.Plains }, card.SubTypes.ToList());
            Assert.AreEqual(0, card.Power);
            Assert.AreEqual(0, card.Toughness);
        }

        [Test]
        public void Test_InitializeCard_always_adds_a_PlayCardAbility()
        {
            var card = CreateAndInitialize("Shock");

            Assert.AreEqual(1, card.Abilities.Count());
            Assert.IsInstanceOf<PlayCardAbility>(card.Abilities.First());
            PlayCardAbility ability = (PlayCardAbility)card.Abilities.First();
            Assert.AreEqual(new ManaCost(0, ManaSymbol.R), ability.ManaCost);
        }

        [Test]
        public void Test_InitializeCard_sets_the_toughness_and_power_for_creature_cards()
        {
            var card = CreateAndInitialize("Mass of Ghouls");

            Assert.AreEqual(Type.Creature, card.Type);
            Assert.AreEqual(5, card.Power);
            Assert.AreEqual(3, card.Toughness);
        }

        [Test]
        public void Test_InitializeCard_sets_the_color_of_cards()
        {
            Assert_Color_is("Icy Manipulator", Color.None);
            Assert_Color_is("Mass of Ghouls", Color.Black);
        }

        #endregion
    }
}
