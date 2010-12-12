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
using System.Windows;
using NUnit.Framework;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class DeckViewModelTests : DeckViewModelTestsBase
    {
        #region Variables

        private DeckViewModel m_deckModel;
        private MockGameFlow m_gameFlow;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            m_deckModel = new DeckViewModel(m_editor, m_deck);
            m_deckModel.Cards.ToString(); // Force creation of cards

            m_gameFlow = MockGameFlow.Use();
        }
        
        [TearDown]
        public void Teardown()
        {
            m_gameFlow.Dispose();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual("My Super Deck", m_deckModel.Name);
            Assert.AreEqual("Frank", m_deckModel.Author);
        }

        [Test]
        public void Test_Can_access_cards()
        {
            Assert.AreEqual(2, m_deckModel.Cards.Count);
            var card1 = m_deckModel.Cards.Single(c => c.Name == m_card1.Card);
            Assert.AreEqual(2, card1.Quantity);

            var card2 = m_deckModel.Cards.Single(c => c.Name == m_card2.Card);
            Assert.AreEqual(1, card2.Quantity);
        }

        [Test]
        public void Test_Can_get_set_IsSelected()
        {
            Assert.IsFalse(m_deckModel.IsSelected);
            m_deckModel.IsSelected = true;
            Assert.IsTrue(m_deckModel.IsSelected);
        }

        [Test]
        public void Test_Can_get_set_IsMouseOver()
        {
            Assert.IsFalse(m_deckModel.IsMouseOver);
            m_deckModel.IsMouseOver = true;
            Assert.IsTrue(m_deckModel.IsMouseOver);
        }
        
        [Test]
        public void Test_ToString()
        {
            Assert.AreEqual("My Super Deck", m_deckModel.ToString());
        }

        [Test]
        public void Test_Can_change_name()
        {
            m_editor.IsEnabled = true;
            m_deckModel.Name = "My new name";
            Assert.AreEqual("My new name", m_deckModel.Name);
            Assert.AreEqual("My new name", m_deck.Name);
        }

        [Test]
        public void Test_Can_change_author()
        {
            m_editor.IsEnabled = true;
            m_deckModel.Author = "My new author";
            Assert.AreEqual("My new author", m_deckModel.Author);
            Assert.AreEqual("My new author", m_deck.Author);
        }

        [Test]
        public void Test_Can_change_description()
        {
            m_editor.IsEnabled = true;
            m_deckModel.Description = "My new description";
            Assert.AreEqual("My new description", m_deckModel.Description);
            Assert.AreEqual("My new description", m_deckModel.DisplayDescription);
            Assert.AreEqual("My new description", m_deck.Description);
        }

        [Test]
        public void Test_DisplayDescription_returns_a_special_string_if_the_description_is_empty()
        {
            m_deck.Description = null;
            Assert.AreEqual("No description", m_deckModel.DisplayDescription);

            m_deck.Description = string.Empty;
            Assert.AreEqual("No description", m_deckModel.DisplayDescription);
        }

        [Test]
        public void Test_DisplayDescription_trims_the_description_when_too_long()
        {
            m_deck.Description = new string('f', 200);

            string expectedString = new string('f', 137) + "...";
            Assert.AreEqual(expectedString, m_deckModel.DisplayDescription);
        }

        [Test]
        public void Test_Cannot_change_properties_when_not_enabled()
        {
            m_editor.IsEnabled = false;
            Assert.Throws<InvalidOperationException>(() => m_deckModel.Name = "My new name");
            Assert.Throws<InvalidOperationException>(() => m_deckModel.Author = "My new name");
            Assert.Throws<InvalidOperationException>(() => m_deckModel.Description = "My new name");
        }

        [Test]
        public void Test_Editing_the_deck_gets_it_dirty()
        {
            m_editor.IsEnabled = true;

            Assert_SetsDirty(() => m_deckModel.Name = "My new name");
            Assert_SetsDirty(() => m_deckModel.Author = "My new name");
            Assert_SetsDirty(() => m_deckModel.Description = "My new name");
        }

        #region Drop

        [Test]
        public void Test_Drop_adds_four_instances_of_the_card_in_the_deck()
        {
            m_editor.IsEnabled = true;

            m_deck.Cards.Clear();
            Assert_SetsDirty(() => m_deckModel.Drop(m_card1, DragDropKeyStates.None));

            Assert.AreEqual(4, m_deck.Cards[m_card1]);
            var cardViewModel = m_deckModel.Cards.Single(model => model.Name == m_card1.Card);
            Assert.AreEqual(4, cardViewModel.Quantity);
        }

        [Test]
        public void Test_Dropping_a_card_thats_already_in_the_deck_increments_the_quantity()
        {
            m_editor.IsEnabled = true;

            Assert_SetsDirty(() => m_deckModel.Drop(m_card1, DragDropKeyStates.None));

            Assert.AreEqual(3, m_deck.Cards[m_card1]);
            var cardViewModel = m_deckModel.Cards.Single(model => model.Name == m_card1.Card);
            Assert.AreEqual(3, cardViewModel.Quantity);
        }

        [Test]
        public void Test_Dropping_with_a_key_modifier_only_adds_one_instance()
        {
            m_editor.IsEnabled = true;

            m_deck.Cards.Clear();
            Assert_SetsDirty(() => m_deckModel.Drop(m_card1, DragDropKeyStates.ShiftKey));

            Assert.AreEqual(1, m_deck.Cards[m_card1]);
            var cardViewModel = m_deckModel.Cards.Single(model => model.Name == m_card1.Card);
            Assert.AreEqual(1, cardViewModel.Quantity);
        }

        [Test]
        public void Test_Cannot_drop_when_readonly()
        {
            m_editor.IsEnabled = false;
            Assert.Throws<InvalidOperationException>(() => m_deckModel.Drop(m_card1, DragDropKeyStates.None));
        }

        #endregion

        #endregion
    }
}