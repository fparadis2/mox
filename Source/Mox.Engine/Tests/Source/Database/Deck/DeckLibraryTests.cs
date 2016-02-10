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
        }

        [Test]
        public void Test_Loading_the_library_loads_all_previously_stored_decks()
        {
            const string DeckContentsA = @"1 Forest";
            const string DeckContentsB = @"1 Plains";

            m_library.Save(new Deck("A"), DeckContentsA);
            m_library.Save(new Deck("B"), DeckContentsB);

            DeckLibrary otherLibrary = new DeckLibrary(m_storage);
            otherLibrary.Load();

            Assert.AreEqual(2, otherLibrary.Decks.Count());
            var deckA = otherLibrary.Decks.Single(d => d.Name == "A");
            var deckB = otherLibrary.Decks.Single(d => d.Name == "B");

            Assert.AreEqual("Forest", deckA.Cards.Single().Card);
            Assert.AreEqual("Plains", deckB.Cards.Single().Card);
        }

        [Test]
        public void Test_GetDeckContents_returns_the_raw_deck_content()
        {
            const string DeckContents = @"
1 Forest
Something bad
----
abcde";

            var deck = m_library.Save(new Deck("My deck"), DeckContents);
            Assert.AreEqual(DeckContents, m_library.GetDeckContents(deck));
        }

        [Test]
        public void Test_Saving_a_deck_will_add_it_to_the_decks_and_persist_it()
        {
            IDeck deck = new Deck("My Deck");

            deck = m_library.Save(deck, null);
            deck = m_library.Save(deck, null); // Saving more than once does nothing particular (except saving twice to storage)

            Assert.Collections.Contains(deck, m_library.Decks);

            Assert.That(m_storage.IsPersisted(deck));
        }

        [Test]
        public void Test_Renaming_a_deck_doesnt_influence_storage()
        {
            const string DeckContents = @"1 Forest";

            var deck = m_library.Save(new Deck("My deck"), DeckContents);
            deck = m_library.Rename(deck, "New name");

            Assert.AreEqual("New name", deck.Name);
            Assert.Collections.Contains(deck, m_library.Decks);

            Assert.That(m_storage.IsPersisted(deck));
            Assert.AreEqual(1, m_storage.PersistedDecksCount);
        }

        [Test]
        public void Test_Deleting_a_deck_will_remove_it_from_the_decks_and_delete_it_from_storage()
        {
            const string DeckContents = @"1 Forest";

            var deck = m_library.Save(new Deck("My deck"), DeckContents);
            m_library.Delete(deck);

            Assert.Collections.IsEmpty(m_library.Decks);
            Assert.IsFalse(m_storage.IsPersisted(deck));
        }

        #endregion
    }
}