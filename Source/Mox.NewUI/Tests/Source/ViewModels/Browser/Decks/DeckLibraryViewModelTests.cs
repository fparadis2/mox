﻿// Copyright (c) François Paradis
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
using Mox.Database;
using NUnit.Framework;

namespace Mox.UI.Browser
{
    public class DeckLibraryViewModelTestsBase
    {
        #region Variables

        protected DeckLibrary m_library;
        protected DeckViewModelEditor m_editor;
        protected DeckLibraryViewModel m_libraryViewModel;

        #endregion

        #region Setup

        [SetUp]
        public virtual void Setup()
        {
            m_library = new DeckLibrary();

            Deck deck1 = new Deck { Name = "Super Deck" };
            Deck deck2 = new Deck { Name = "Ordinary Deck" };

            m_library.Save(deck1);
            m_library.Save(deck2);

            m_editor = new DeckViewModelEditor(new CardDatabase(), null);

            m_libraryViewModel = new DeckLibraryViewModel(m_library, m_editor);
        }

        #endregion
    }

    [TestFixture]
    public class DeckLibraryViewModelTests : DeckLibraryViewModelTestsBase
    {
        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_editor, m_libraryViewModel.Editor);
            Assert.Collections.CountEquals(2, m_libraryViewModel.Decks);
            Assert.IsNull(m_libraryViewModel.Filter);
        }

        [Test]
        public void Test_Can_apply_simple_text_filter()
        {
            var view = m_libraryViewModel.DecksViewSource.View.Cast<DeckViewModel>();
            Assert.Collections.CountEquals(2, view);

            m_libraryViewModel.Filter = "Super";
            Assert.AreEqual("Super", m_libraryViewModel.Filter);

            Assert.Collections.CountEquals(1, view);
            Assert.AreEqual("Super Deck", view.Single().Name);
        }

        [Test]
        public void Test_Can_get_set_SelectedDeck()
        {
            var deck = m_libraryViewModel.Decks.First();

            Assert.IsNull(m_libraryViewModel.SelectedDeck);
            Assert.IsFalse(deck.IsSelected);

            m_libraryViewModel.SelectedDeck = deck;
            Assert.AreEqual(deck, m_libraryViewModel.SelectedDeck);
            Assert.True(deck.IsSelected);

            m_libraryViewModel.SelectedDeck = null;
            Assert.IsNull(m_libraryViewModel.SelectedDeck);
            Assert.IsFalse(deck.IsSelected);
        }

        [Test]
        public void Test_Add_adds_a_deck_to_the_library_and_sets_it_as_selected_deck()
        {
            Deck deck = new Deck { Name = "New Deck", Author = "Jack" };

            var deckModel = m_libraryViewModel.Add(deck);

            Assert.AreEqual("Jack", deck.Author);
            Assert.AreEqual(deck, deckModel.Deck);

            Assert.Collections.Contains(deck, m_library.Decks);
            Assert.Collections.Contains(deckModel, m_libraryViewModel.Decks);
            Assert.AreEqual(deckModel, m_libraryViewModel.SelectedDeck);
        }

        [Test]
        public void Test_Cannot_add_a_null_deck()
        {
            Assert.Throws<ArgumentNullException>(() => m_libraryViewModel.Add(null));
        }

        [Test]
        public void Test_Add_fills_author_if_not_provided()
        {
            m_editor.UserName = "John";

            Deck deck = new Deck { Name = "New Deck" };

            m_libraryViewModel.Add(deck);

            Assert.AreEqual("John", deck.Author);
        }

        [Test]
        public void Test_Add_fills_the_name_if_not_provided()
        {
            Deck deck = new Deck();

            m_libraryViewModel.Add(deck);

            Assert.AreEqual("New Deck", deck.Name);
        }

        #endregion
    }
}
