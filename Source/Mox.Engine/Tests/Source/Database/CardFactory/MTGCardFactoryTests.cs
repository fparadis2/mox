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
using Rhino.Mocks;

using Is = Rhino.Mocks.Constraints.Is;

namespace Mox.Database
{
    [TestFixture]
    public class MTGCardFactoryTests : BaseGameTests
    {
        #region Inner Types

        public abstract class MockMTGCardFactory : MTGCardFactory
        {
            protected override sealed void  Initialize(Card card, InitializationContext context)
            {
                InitializeImpl(card, context);
            }

            public abstract void InitializeImpl(Card card, InitializationContext context);
        }

        #endregion

        #region Variables

        private MockMTGCardFactory m_factory;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_factory = m_mockery.PartialMock<MockMTGCardFactory>();
        }

        #endregion

        #region Utilities

        private void Expect_InitializeImpl(Card card)
        {
            m_factory.InitializeImpl(card, null);

            LastCall.Constraints(Is.Equal(card), Is.Matching<MTGCardFactory.InitializationContext>(context => context != null && context.PlayCardAbility == card.Abilities.First()));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_InitializeCard_uses_the_master_database_to_initialize_the_card()
        {
            m_card = CreateCard(m_playerA, "Plains");

            Expect_InitializeImpl(m_card);

            m_mockery.Test(() => m_factory.InitializeCard(m_card));

            Assert.AreEqual(Type.Land, m_card.Type);
            Assert.Collections.AreEqual(new[] { SubType.Plains }, m_card.SubTypes.ToList());
            Assert.AreEqual(0, m_card.Power);
            Assert.AreEqual(0, m_card.Toughness);
        }

        [Test]
        public void Test_InitializeCard_always_adds_a_PlayCardAbility()
        {
            m_card = CreateCard(m_playerA, "Shock");

            Expect_InitializeImpl(m_card);

            m_mockery.Test(() => m_factory.InitializeCard(m_card));

            Assert.AreEqual(1, m_card.Abilities.Count());
            Assert.IsInstanceOf<PlayCardAbility>(m_card.Abilities.First());
            PlayCardAbility ability = (PlayCardAbility)m_card.Abilities.First();
            Assert.AreEqual(new ManaCost(0, ManaSymbol.R), ability.ManaCost);
        }

        [Test]
        public void Test_InitializeCard_throws_if_the_card_is_unknown()
        {
            m_mockery.ReplayAll();
            Assert.Throws<ArgumentException>(() => m_factory.InitializeCard(m_card));
        }

        [Test]
        public void Test_InitializeCard_sets_the_toughness_and_power_for_creature_cards()
        {
            m_card = CreateCard(m_playerA, "Mass of Ghouls");

            Expect_InitializeImpl(m_card);

            m_mockery.Test(() => m_factory.InitializeCard(m_card));

            Assert.AreEqual(Type.Creature, m_card.Type);
            Assert.AreEqual(5, m_card.Power);
            Assert.AreEqual(3, m_card.Toughness);
        }

        private void Assert_Color_is(string cardName, Color color)
        {
            m_card = CreateCard(m_playerA, cardName);

            Expect_InitializeImpl(m_card);

            m_mockery.Test(() => m_factory.InitializeCard(m_card));

            Assert.AreEqual(color, m_card.Color);
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
