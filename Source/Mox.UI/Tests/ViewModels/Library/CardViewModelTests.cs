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
using Mox.Database;
using NUnit.Framework;

namespace Mox.UI.Library
{
    [TestFixture]
    public class CardViewModelTests
    {
        #region Variables

        private CardInfo m_card;
        private CardViewModel m_cardModel;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_card = new CardDatabase().AddCard("MyCard", "RBG", Color.Red, SuperType.Basic, Type.Artifact, new[] { SubType.Advisor, SubType.Antelope }, "*", "2", "Hello world!");
            var set1 = m_card.Database.AddSet("SET", "My Set", "A block", DateTime.Now);
            var older = m_card.Database.AddSet("OLDER", "Older Set", "A block", DateTime.Now.Subtract(TimeSpan.FromDays(2)));
            m_card.Database.AddCardInstance(m_card, older, 0, Rarity.Common, 3, "Hello");
            m_card.Database.AddCardInstance(m_card, set1, 0, Rarity.Rare, 4, "Hello");
            m_cardModel = new CardViewModel(m_card);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_card, m_cardModel.Card);

            Assert.AreEqual("MyCard", m_cardModel.Name);
            Assert.AreEqual("Basic Artifact — Advisor Antelope", m_cardModel.TypeLine);
            Assert.AreEqual("RBG", m_cardModel.ManaCost);
            Assert.AreEqual("Hello world!", m_cardModel.Rules);

            Assert.AreEqual(2, m_cardModel.Printings.Count);
            Assert.IsTrue(m_cardModel.HasMoreThanOnePrinting);
        }

        [Test]
        public void Test_Change_notifications_are_correctly_triggered()
        {
            Assert.ThatAllPropertiesOn(m_cardModel).SetValue(c => c.CurrentPrinting, m_cardModel.Printings[1]).RaiseChangeNotification();
        }

        [Test]
        public void Test_ToString()
        {
            Assert.AreEqual("MyCard", m_cardModel.ToString());
        }

        [Test]
        public void Test_CurrentEdition_returns_the_latest_edition_by_default()
        {
            var latestEdition = m_cardModel.CurrentPrinting;
            Assert.AreEqual("My Set", latestEdition.SetName);
        }

        [Test]
        public void Test_can_set_CurrentEdition()
        {
            m_cardModel.CurrentPrinting = m_cardModel.Printings[1];
            Assert.AreEqual(m_cardModel.Printings[1], m_cardModel.CurrentPrinting);
        }

        [Test]
        public void Test_CardIdentifier_returns_a_basic_identifier_with_no_set_info_if_card_is_on_latest_edition()
        {
            Assert.AreEqual(new CardIdentifier { Card = "MyCard" }, m_cardModel.CardIdentifier);
        }

        [Test]
        public void Test_CardIdentifier_returns_a_complete_identifier_with_set_info_otherwise()
        {
            var edition = m_cardModel.Printings[1];
            m_cardModel.CurrentPrinting = edition;
            Assert.AreEqual(new CardIdentifier { Card = "MyCard", Set = edition.SetIdentifier }, m_cardModel.CardIdentifier);
        }

        #endregion
    }
}