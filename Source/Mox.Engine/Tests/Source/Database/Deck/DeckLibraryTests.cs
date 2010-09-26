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
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Mox.Database
{
    [TestFixture]
    public class DeckLibraryTests
    {
        #region Variables

        private MemoryDeckStorageStrategy m_storage;
        private DeckLibrary m_library;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_storage = new MemoryDeckStorageStrategy();
            m_library = new DeckLibrary(m_storage);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.Collections.IsEmpty(m_library.Decks);
            Assert.That(m_library.Decks.IsReadOnly);
        }

        [Test]
        public void Test_Saving_a_deck_will_add_it_to_the_decks_and_persist_it()
        {
            Deck deck = new Deck { Name = "My Deck" };

            m_library.Save(deck);
            m_library.Save(deck); // Saving more than once does nothing particular (except saving twice to storage)

            Assert.Collections.Contains(deck, m_library.Decks);

            Assert.That(m_storage.IsPersisted(deck));
        }

        [Test]
        public void Test_Can_save_a_cloned_deck()
        {
            Deck deck = new Deck { Name = "My Deck" };

            m_library.Save(deck);

            deck = deck.Clone();

            m_library.Save(deck);

            Assert.Collections.Contains(deck, m_library.Decks);

            Assert.That(m_storage.IsPersisted(deck));
        }

        [Test]
        public void Test_Renaming_a_deck_doesnt_influence_storage()
        {
            Deck deck = new Deck { Name = "My Deck" };

            m_library.Save(deck);
            deck.Name = "My new name";
            m_library.Save(deck); // Saving more than once does nothing particular (except saving twice to storage)

            Assert.Collections.Contains(deck, m_library.Decks);

            Assert.That(m_storage.IsPersisted(deck));
            Assert.AreEqual(1, m_storage.PersistedDecksCount);
        }

        [Test]
        public void Test_Deleting_a_deck_will_remove_it_from_the_decks_and_delete_it_from_storage()
        {
            Deck deck = new Deck { Name = "My Deck" };

            m_library.Save(deck);
            m_library.Delete(deck);

            Assert.Collections.IsEmpty(m_library.Decks);

            Assert.IsFalse(m_storage.IsPersisted(deck));
        }

        [Test]
        public void Test_Loading_the_library_loads_all_previously_stored_decks()
        {
            Deck deck1 = new Deck { Name = "My deck" };
            Deck deck2 = new Deck { Name = "My other deck" };

            m_library.Save(deck1);
            m_library.Save(deck2);

            DeckLibrary otherLibrary = new DeckLibrary(m_storage);
            otherLibrary.Load();

            Assert.AreEqual(2, otherLibrary.Decks.Count);
            Assert.That(otherLibrary.Decks.Any(d => d.Name == "My deck"));
            Assert.That(otherLibrary.Decks.Any(d => d.Name == "My other deck"));
        }

        #endregion
    }
}