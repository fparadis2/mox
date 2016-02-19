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
using NUnit.Framework;

namespace Mox.Database
{
    [TestFixture]
    public class DeckLibraryTests
    {
        #region Variables

        private MockStorageStrategy m_storage;
        private DeckLibrary m_library;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_storage = new MockStorageStrategy();
            m_library = new DeckLibrary(m_storage);
        }

        private void CheckValidateName(IDeck oldDeck, string name)
        {
            string oldName = name;

            string error;
            Assert.That(m_library.ValidateDeckName(oldDeck, ref name, out error));
            Assert.AreEqual(oldName, name);
        }

        private string CheckValidateNameFails(IDeck oldDeck, string name)
        {
            string originalName = name;

            string error;
            Assert.IsFalse(m_library.ValidateDeckName(oldDeck, ref name, out error));
            Assert.IsNotNullOrEmpty(error);

            Assert.IsNull(m_library.Save(oldDeck, originalName, "Contents"));

            return name;
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

            m_library.Create("A", DeckContentsA);
            m_library.Create("B", DeckContentsB);

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

            var deck = m_library.Create("My deck", DeckContents);
            Assert.AreEqual(DeckContents, m_library.GetDeckContents(deck));
        }

        [Test]
        public void Test_ValidateNewDeckName_returns_false_for_an_empty_name()
        {
            CheckValidateNameFails(null, string.Empty);
            CheckValidateNameFails(null, null);
        }

        [Test]
        public void Test_ValidateNewDeckName_returns_false_if_another_deck_already_has_that_name()
        {
            m_library.Create("My deck", null);
            Assert.AreEqual("My deck (1)", CheckValidateNameFails(null, "My deck"));

            m_library.Create("My deck (1)", null);
            Assert.AreEqual("My deck (2)", CheckValidateNameFails(null, "My deck"));
            Assert.AreEqual("My deck (2)", CheckValidateNameFails(null, "My deck (1)"));
            CheckValidateName(null, "My deck (3)");
        }

        [Test]
        public void Test_ValidateNewDeckName_returns_true_if_were_not_changing_the_name_of_the_deck()
        {
            var deck = m_library.Create("My deck", null);
            CheckValidateName(deck, "My deck");
        }

        [Test]
        public void Test_ValidateNewDeckName_returns_false_for_a_new_deck_with_a_valid_name()
        {
            CheckValidateName(null, "Valid");
        }

        [Test]
        public void Test_ValidateNewDeckName_returns_false_if_the_storage_says_its_an_invalid_name()
        {
            m_storage.InvalidNames.Add("Mucho");
            Assert.AreEqual("Mucho Potato", CheckValidateNameFails(null, "Mucho"));
        }

        [Test]
        public void Test_Saving_a_deck_will_add_it_to_the_decks_and_persist_it_if_it_doesnt_exist()
        {
            var deck = m_library.Save(null, "New", null);
            Assert.Collections.Contains(deck, m_library.Decks);
            Assert.That(m_storage.IsPersisted(deck));

            // Saving more than once does nothing particular (except saving twice to storage)
            deck = m_library.Save(deck, deck.Name, null);
            Assert.Collections.Contains(deck, m_library.Decks);
            Assert.That(m_storage.IsPersisted(deck));
        }

        [Test]
        public void Test_Saving_a_deck_will_update_the_storage_with_the_new_contents()
        {
            var deck = m_library.Create("New", null);
            deck = m_library.Save(deck, deck.Name, "New Content");
            Assert.AreEqual("New Content", m_storage.GetDeckContents(deck));
        }

        [Test]
        public void Test_Renaming_a_deck()
        {
            var deck = m_library.Create("My Deck", null);
            var renamedDeck = m_library.Save(deck, "New Name", null);

            Assert.That(!m_library.Decks.Contains(deck));
            Assert.That(!m_storage.IsPersisted(deck));

            Assert.Collections.Contains(renamedDeck, m_library.Decks);
            Assert.That(m_storage.IsPersisted(renamedDeck));
        }

        [Test]
        public void Test_Deleting_a_deck_will_remove_it_from_the_decks_and_delete_it_from_storage()
        {
            const string DeckContents = @"1 Forest";

            var deck = m_library.Create("My deck", DeckContents);
            m_library.Delete(deck);

            Assert.Collections.IsEmpty(m_library.Decks);
            Assert.IsFalse(m_storage.IsPersisted(deck));
        }

        #endregion

        #region Inner Types

        private class MockStorageStrategy : MemoryDeckStorageStrategy
        {
            public readonly List<string> InvalidNames = new List<string>();

            public override bool ValidateDeckName(ref string name, out string error)
            {
                if (InvalidNames.Contains(name))
                {
                    error = "Invalid!!!";
                    name += " Potato";
                    return false;
                }

                return base.ValidateDeckName(ref name, out error);
            }
        }

        #endregion
    }
}