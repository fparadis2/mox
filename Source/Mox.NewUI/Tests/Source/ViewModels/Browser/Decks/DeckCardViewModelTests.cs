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

namespace Mox.UI.Browser
{
    [TestFixture]
    public class DeckCardViewModelTests : DeckViewModelTestsBase
    {
        #region Variables

        private DeckViewModel m_owner;
        private DeckCardViewModel m_model;

        #endregion

        #region Setup

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            m_owner = new DeckViewModel(m_deck, m_editor);
            m_model = m_owner.Cards.Single(c => c.CardIdentifier == m_card1);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(2, m_model.Quantity);
        }

        [Test]
        public void Test_PropertyChangeNotification()
        {
            m_editor.IsEnabled = true;
            Assert.ThatAllPropertiesOn(m_model).SetValue(c => c.CurrentEdition, m_model.Editions[1]).RaiseChangeNotification();
        }

        [Test]
        public void Test_Can_change_quantity()
        {
            m_editor.IsEnabled = true;
            m_model.Quantity = 7;
            Assert.AreEqual(7, m_model.Quantity);
            Assert.AreEqual(7, m_deck.Cards[m_card1]);
        }

        [Test]
        public void Test_Setting_an_invalid_quantity_does_nothing()
        {
            m_editor.IsEnabled = true;
            m_model.Quantity = -1;
            Assert.AreEqual(0, m_model.Quantity);
            Assert.That(!m_owner.Cards.Contains(m_model));
        }

        [Test]
        public void Test_Cannot_change_properties_when_not_enabled()
        {
            m_editor.IsEnabled = false;
            Assert.Throws<InvalidOperationException>(() => m_model.Quantity = 10);
        }

        [Test]
        public void Test_Setting_the_quantity_sets_the_editor_dirty()
        {
            m_editor.IsEnabled = true;
            Assert_SetsDirty(() => m_model.Quantity = 10);
        }

        [Test]
        public void Test_Setting_the_quantity_updates_the_group_Quantity()
        {
            m_editor.IsEnabled = true;

            Assert.AreEqual(3, m_model.Group.Quantity, "Sanity check");
            m_model.Quantity = 10;
            Assert.AreEqual(11, m_model.Group.Quantity);
        }

        [Test]
        public void Test_Setting_the_same_quantity_doesnt_set_the_editor_dirty()
        {
            m_editor.IsEnabled = true;
            m_model.Quantity = 2;
            Assert.IsFalse(m_editor.IsDirty);
        }

        [Test]
        public void Test_Increment_adds_one_card()
        {
            m_editor.IsEnabled = true;
            m_model.Increment();
            Assert.AreEqual(3, m_model.Quantity);
        }

        [Test]
        public void Test_Decrement_removes_one_card()
        {
            m_editor.IsEnabled = true;
            m_model.Decrement();
            Assert.AreEqual(1, m_model.Quantity);
        }

        [Test]
        public void Test_Decrement_removes_the_whole_card_if_only_one_card()
        {
            m_editor.IsEnabled = true;
            m_model.Quantity = 1;
            Assert.That(m_owner.Cards.Contains(m_model), "Sanity check");
            m_model.Decrement();
            Assert.That(!m_owner.Cards.Contains(m_model));
            Assert.AreEqual(0, m_model.Quantity);
            Assert.AreEqual(1, m_model.Group.Quantity);
        }

        #endregion
    }
}
