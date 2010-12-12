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
using System.Collections.Generic;
using System.Linq;
using Mox.Database;
using NUnit.Framework;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class DeckLibraryViewModelTests
    {
        #region Variables

        private DeckLibrary m_library;
        private DeckLibraryViewModel m_collection;

        private MockDeckViewModelEditor m_editor;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_library = new DeckLibrary();

            Deck deck1 = new Deck { Name = "Super Deck" };
            Deck deck2 = new Deck { Name = "Ordinary Deck" };

            m_library.Save(deck1);
            m_library.Save(deck2);

            m_editor = new MockDeckViewModelEditor(new CardDatabase(), m_library);

            m_collection = new DeckLibraryViewModel(m_editor);
        }

        #endregion

        #region Utilities

        private IEnumerable<DeckViewModel> View
        {
            get { return m_collection.DecksViewSource.View.Cast<DeckViewModel>(); }
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.Collections.CountEquals(2, View);
            Assert.IsNull(m_collection.Filter);
        }

        [Test]
        public void Test_Can_apply_simple_text_filter()
        {
            m_collection.Filter = "Super";
            Assert.AreEqual("Super", m_collection.Filter);

            Assert.Collections.CountEquals(1, View);
            Assert.AreEqual("Super Deck", View.First().Name);
        }

        [Test]
        public void Test_Can_get_set_SelectedDeck()
        {
            Assert.IsNull(m_collection.SelectedDeck);
            m_collection.SelectedDeck = View.First();
            Assert.AreEqual(View.First(), m_collection.SelectedDeck);
        }

        [Test]
        public void Test_Add_adds_a_deck_to_the_library_and_sets_it_as_selected_deck()
        {
            Deck deck = new Deck { Name = "New Deck", Author = "Jack" };

            var deckModel = m_collection.Add(deck);

            Assert.AreEqual("Jack", deck.Author);
            Assert.AreEqual(deck, deckModel.Deck);

            Assert.Collections.Contains(deck, m_library.Decks);
            Assert.AreEqual(deckModel, m_collection.SelectedDeck);
        }

        [Test]
        public void Test_Cannot_add_a_null_deck()
        {
            Assert.Throws<ArgumentNullException>(() => m_collection.Add(null));
        }

        [Test]
        public void Test_Add_fills_author_if_not_provided()
        {
            m_editor.UserName = "John";

            Deck deck = new Deck { Name = "New Deck" };

            m_collection.Add(deck);

            Assert.AreEqual("John", deck.Author);
        }

        [Test]
        public void Test_Add_fills_the_name_if_not_provided()
        {
            Deck deck = new Deck();

            m_collection.Add(deck);

            Assert.AreEqual("New Deck", deck.Name);
        }

        #endregion
    }
}
