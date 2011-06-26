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
using Rhino.Mocks;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class DeckViewModelTests : DeckViewModelTestsBase
    {
        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual("My Super Deck", m_deckViewModel.Name);
            Assert.AreEqual("Frank", m_deckViewModel.Author);
        }

        [Test]
        public void Test_PropertyChangeNotification()
        {
            m_deckViewModel.BeginEdit();
            Assert.ThatAllPropertiesOn(m_deckViewModel).RaiseChangeNotification();
        }

        [Test]
        public void Test_Can_access_cards()
        {
            Assert.AreEqual(2, m_deckViewModel.Cards.Count);
            var card1 = m_deckViewModel.Cards.Single(c => c.Name == m_card1.Card);
            Assert.AreEqual(2, card1.Quantity);

            var card2 = m_deckViewModel.Cards.Single(c => c.Name == m_card2.Card);
            Assert.AreEqual(1, card2.Quantity);
        }

        [Test]
        public void Test_Can_get_set_IsSelected()
        {
            Assert.IsFalse(m_deckViewModel.IsSelected);
            m_deckViewModel.IsSelected = true;
            Assert.IsTrue(m_deckViewModel.IsSelected);
        }

        [Test]
        public void Test_Can_get_set_IsMouseOver()
        {
            Assert.IsFalse(m_deckViewModel.IsMouseOver);
            m_deckViewModel.IsMouseOver = true;
            Assert.IsTrue(m_deckViewModel.IsMouseOver);
        }
        
        [Test]
        public void Test_ToString()
        {
            Assert.AreEqual("My Super Deck", m_deckViewModel.ToString());
        }

        [Test]
        public void Test_Can_change_name()
        {
            m_deckViewModel.BeginEdit();
            m_deckViewModel.Name = "My new name";
            Assert.AreEqual("My new name", m_deckViewModel.Name);
            Assert.AreEqual("My new name", Deck.Name);
        }

        [Test]
        public void Test_Cannot_have_an_empty_name()
        {
            m_deckViewModel.BeginEdit();
            m_deckViewModel.Name = null;
            Assert.AreEqual(null, m_deckViewModel.Name);
            Assert.ThatProperty(m_deckViewModel, d => d.Name).FailsValidation("Deck Name cannot be empty.");
        }

        [Test]
        public void Test_Errors_are_cleared_when_cancelling_edit()
        {
            m_deckViewModel.BeginEdit();
            m_deckViewModel.Name = null;
            Assert.ThatProperty(m_deckViewModel, d => d.Name).FailsValidation("Deck Name cannot be empty.");
            m_deckViewModel.CancelEdit();
            Assert.ThatProperty(m_deckViewModel, d => d.Name).PassesValidation();
        }

        [Test]
        public void Test_Can_change_author()
        {
            m_deckViewModel.BeginEdit();
            m_deckViewModel.Author = "My new author";
            Assert.AreEqual("My new author", m_deckViewModel.Author);
            Assert.AreEqual("My new author", Deck.Author);
        }

        [Test]
        public void Test_Cannot_have_an_empty_author()
        {
            m_deckViewModel.BeginEdit();
            m_deckViewModel.Author = null;
            Assert.AreEqual(null, m_deckViewModel.Author);
            Assert.ThatProperty(m_deckViewModel, d => d.Author).FailsValidation("Deck Author cannot be empty.");
        }

        [Test]
        public void Test_Can_change_description()
        {
            m_deckViewModel.BeginEdit();
            m_deckViewModel.Description = "My new description";
            Assert.AreEqual("My new description", m_deckViewModel.Description);
            Assert.AreEqual("My new description", Deck.Description);
        }

        [Test]
        public void Test_Editing_the_deck_gets_it_dirty()
        {
            m_deckViewModel.BeginEdit();

            Assert_SetsDirty(() => m_deckViewModel.Name = "My new name");
            Assert_SetsDirty(() => m_deckViewModel.Author = "My new name");
            Assert_SetsDirty(() => m_deckViewModel.Description = "My new name");
        }

        [Test]
        public void Test_BeginEdit_puts_in_Editing_mode()
        {
            Assert.That(!m_deckViewModel.IsEditing);

            m_deckViewModel.BeginEdit();

            Assert.That(m_deckViewModel.IsEditing);

            m_deckViewModel.EndEdit();

            Assert.That(!m_deckViewModel.IsEditing);
        }

        [Test]
        public void Test_CancelEdit_reverts_all_changes()
        {
            m_deckViewModel.BeginEdit();

            m_deckViewModel.Name = "My new name";
            m_deckViewModel.Author = "My new name";
            m_deckViewModel.Description = "My new name";

            m_deckViewModel.CancelEdit();

            Assert.That(!m_deckViewModel.IsEditing);

            Assert.AreEqual("My Super Deck", m_deckViewModel.Name);
            Assert.AreEqual("Frank", m_deckViewModel.Author);
            Assert.IsNull(m_deckViewModel.Description);

            Assert.AreEqual("My Super Deck", Deck.Name);
            Assert.AreEqual("Frank", Deck.Author);
            Assert.IsNull(Deck.Description);
        }

        [Test]
        public void Test_EndEdit_commits_all_changes()
        {
            m_deckViewModel.BeginEdit();

            m_deckViewModel.Name = "My new name";
            m_deckViewModel.Author = "My new author";
            m_deckViewModel.Description = "My new description";

            m_deckViewModel.EndEdit();

            Assert.That(!m_deckViewModel.IsEditing);

            Assert.AreEqual("My new name", m_deckViewModel.Name);
            Assert.AreEqual("My new author", m_deckViewModel.Author);
            Assert.AreEqual("My new description", m_deckViewModel.Description);

            Assert.AreEqual("My new name", Deck.Name);
            Assert.AreEqual("My new author", Deck.Author);
            Assert.AreEqual("My new description", Deck.Description);
        }

        [Test]
        public void Test_Cannot_change_properties_when_not_editing()
        {
            Assert.That(!m_deckViewModel.IsEditing, "Sanity check");
            Assert.Throws<InvalidOperationException>(() => m_deckViewModel.Name = "My new name");
            Assert.Throws<InvalidOperationException>(() => m_deckViewModel.Author = "My new name");
            Assert.Throws<InvalidOperationException>(() => m_deckViewModel.Description = "My new name");
        }

        #region Drop

        [Test]
        public void Test_Drop_adds_four_instances_of_the_card_in_the_deck()
        {
            Deck.Cards.Clear();
            m_deckViewModel.Cards.Clear();

            m_deckViewModel.BeginEdit();

            Assert_SetsDirty(() => m_deckViewModel.Drop(m_card1, DragDropKeyStates.None));

            Assert.AreEqual(4, Deck.Cards[m_card1]);
            var cardViewModel = m_deckViewModel.Cards.Single(model => model.Name == m_card1.Card);
            Assert.AreEqual(4, cardViewModel.Quantity);
        }

        [Test]
        public void Test_Canceling_a_drop_correctly_removes_the_card()
        {
            Deck.Cards.Clear();
            m_deckViewModel.Cards.Clear();

            m_deckViewModel.BeginEdit();

            Assert_SetsDirty(() => m_deckViewModel.Drop(m_card1, DragDropKeyStates.None));

            Assert.Collections.CountEquals(1, m_deckViewModel.Cards);
            m_deckViewModel.CancelEdit();
            Assert.Collections.IsEmpty(m_deckViewModel.Cards);
        }

        [Test]
        public void Test_Dropping_a_card_thats_already_in_the_deck_increments_the_quantity()
        {
            m_deckViewModel.BeginEdit();

            Assert_SetsDirty(() => m_deckViewModel.Drop(m_card1, DragDropKeyStates.None));

            Assert.AreEqual(3, Deck.Cards[m_card1]);
            var cardViewModel = m_deckViewModel.Cards.Single(model => model.Name == m_card1.Card);
            Assert.AreEqual(3, cardViewModel.Quantity);
        }

        [Test]
        public void Test_Drop_updates_the_group_quantity()
        {
            m_deckViewModel.BeginEdit();

            Deck.Cards.Clear();
            var model1 = m_deckViewModel.Drop(m_card1, DragDropKeyStates.None);
            var existingGroup = model1.Group;
            Assert.AreEqual(4, existingGroup.Quantity);

            var model2 = m_deckViewModel.Drop(m_card2, DragDropKeyStates.None);
            Assert.AreEqual(existingGroup, model2.Group);
            Assert.AreEqual(8, model2.Group.Quantity);
        }

        [Test]
        public void Test_Dropping_with_a_key_modifier_only_adds_one_instance()
        {
            m_deckViewModel.BeginEdit();

            Deck.Cards.Clear();
            Assert_SetsDirty(() => m_deckViewModel.Drop(m_card1, DragDropKeyStates.ShiftKey));

            Assert.AreEqual(1, Deck.Cards[m_card1]);
            var cardViewModel = m_deckViewModel.Cards.Single(model => model.Name == m_card1.Card);
            Assert.AreEqual(1, cardViewModel.Quantity);
        }

        [Test]
        public void Test_Cannot_drop_when_readonly()
        {
            Assert.IsFalse(m_deckViewModel.IsEditing);
            Assert.Throws<InvalidOperationException>(() => m_deckViewModel.Drop(m_card1, DragDropKeyStates.None));
        }

        [Test]
        public void Test_Drop_selects_the_dropped_card()
        {
            m_deckViewModel.BeginEdit();

            Deck.Cards.Clear();
            var card = m_deckViewModel.Drop(m_card1, DragDropKeyStates.None);
            Assert.That(card.IsSelected);
        }

        #endregion

        #endregion
    }
}