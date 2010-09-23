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

using NUnit.Framework;

namespace Mox.Database
{
    [TestFixture]
    public class CompoundCardFactoryTests : BaseGameTests
    {
        #region Inner Types

        private class MockCompoundCardFactory : CompoundCardFactory
        {
            public new void Register(string card, ICardFactory cardFactory)
            {
                base.Register(card, cardFactory);
            }
        }

        #endregion

        #region Variables

        private ICardFactory m_subFactory;
        private MockCompoundCardFactory m_factory;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_factory = new MockCompoundCardFactory();
            m_subFactory = m_mockery.StrictMock<ICardFactory>();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Does_nothing_when_the_card_is_unknown()
        {
            m_factory.InitializeCard(m_card);
        }

        [Test]
        public void Test_Can_register_sub_factories_per_card()
        {
            m_factory.Register(m_card.Name, m_subFactory);

            m_subFactory.InitializeCard(m_card);

            m_mockery.Test(() => m_factory.InitializeCard(m_card));
        }

        [Test]
        public void Test_IsDefined_returns_true_if_there_is_a_sub_factory_for_the_given_card()
        {
            Assert.IsFalse(m_factory.IsDefined(m_card.Name));
            m_factory.Register(m_card.Name, m_subFactory);
            Assert.IsTrue(m_factory.IsDefined(m_card.Name));
        }

        [Test]
        public void Test_Count_returns_the_number_of_defined_cards()
        {
            Assert.AreEqual(0, m_factory.Count);
            m_factory.Register(m_card.Name, m_subFactory);
            Assert.AreEqual(1, m_factory.Count);
        }

        [Test]
        public void Test_Cannot_register_with_invalid_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => m_factory.Register(null, m_subFactory));
            Assert.Throws<ArgumentNullException>(() => m_factory.Register(string.Empty, m_subFactory));
            Assert.Throws<ArgumentNullException>(() => m_factory.Register("My Card", null));
        }

        [Test]
        public void Test_Cannot_register_two_factories_for_the_same_card()
        {
            m_factory.Register("MyCard", m_subFactory);
            Assert.Throws<ArgumentException>(() => m_factory.Register("MyCard", m_subFactory));
        }

        #endregion
    }
}
