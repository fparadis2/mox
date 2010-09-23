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
using System.IO;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;

namespace Mox.Database
{
    [TestFixture]
    public class DeckTests
    {
        #region Variables

        private Deck m_deck;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_deck = new Deck();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.Collections.IsEmpty(m_deck.Cards);
            Assert.AreNotEqual(Guid.Empty, m_deck.Guid);
        }

        [Test]
        public void Test_Can_add_cards()
        {
            CardIdentifier card = new CardIdentifier { Card = "Hello" };

            m_deck.Cards.Add(card);
            m_deck.Cards.Add(card); // Can add more than once

            Assert.Collections.AreEquivalent(new[] { card, card }, m_deck.Cards);
            Assert.AreEqual(2, m_deck.Cards[card]);
        }

        [Test]
        public void Test_Can_add_cards_in_batch()
        {
            CardIdentifier card = new CardIdentifier { Card = "Hello" };

            m_deck.Cards.Add(card, 3);

            Assert.Collections.AreEquivalent(new[] { card, card, card }, m_deck.Cards);
            Assert.AreEqual(3, m_deck.Cards[card]);
        }

        [Test]
        public void Test_Can_remove_cards()
        {
            CardIdentifier card = new CardIdentifier { Card = "Hello" };

            m_deck.Cards.Add(card, 3);
            m_deck.Cards.Remove(card, 2);

            Assert.Collections.AreEquivalent(new[] { card }, m_deck.Cards);
            Assert.AreEqual(1, m_deck.Cards[card]);
        }

        [Test]
        public void Test_Cannot_have_negative_count_of_cards()
        {
            CardIdentifier card = new CardIdentifier { Card = "Hello" };

            m_deck.Cards.Add(card, 1);
            m_deck.Cards.Remove(card, 2);

            Assert.AreEqual(0, m_deck.Cards[card]);
        }

        [Test]
        public void Test_Can_set_number_directly()
        {
            CardIdentifier card = new CardIdentifier { Card = "Hello" };

            m_deck.Cards[card] = 2;

            Assert.Collections.AreEquivalent(new[] { card, card }, m_deck.Cards);
            Assert.Collections.AreEquivalent(new[] { card }, m_deck.Cards.Keys);
            Assert.AreEqual(2, m_deck.Cards[card]);
        }

        [Test]
        public void Test_ContainsKey_returns_true_if_deck_contains_at_least_one_instance_of_the_card()
        {
            CardIdentifier card = new CardIdentifier { Card = "Hello" };

            m_deck.Cards[card] = 2;
            Assert.IsTrue(m_deck.Cards.ContainsKey(card));

            m_deck.Cards[card] = 0;
            Assert.IsFalse(m_deck.Cards.ContainsKey(card));
        }

        [Test]
        public void Test_Cannot_set_negative_number()
        {
            CardIdentifier card = new CardIdentifier { Card = "Hello" };

            Assert.Throws<ArgumentException>(() => m_deck.Cards[card] = -2);
        }

        [Test]
        public void Test_Cards_are_indexed_by_set()
        {
            CardIdentifier card = new CardIdentifier { Card = "Hello" };
            CardIdentifier sameCardInSpecificSet = new CardIdentifier { Card = "Hello", Set = "MySet" };

            m_deck.Cards.Add(card, 3);
            m_deck.Cards.Add(sameCardInSpecificSet, 1);

            Assert.Collections.AreEquivalent(new[] { card, card, card, sameCardInSpecificSet }, m_deck.Cards);
            Assert.Collections.AreEquivalent(new[] { card, sameCardInSpecificSet }, m_deck.Cards.Keys);
            Assert.AreEqual(3, m_deck.Cards[card]);
            Assert.AreEqual(1, m_deck.Cards[sameCardInSpecificSet]);
        }

        [Test]
        public void Test_Cards_can_be_accessed_by_string()
        {
            CardIdentifier card = new CardIdentifier { Card = "Hello" };
            CardIdentifier sameCardInSpecificSet = new CardIdentifier { Card = "Hello", Set = "MySet" };

            m_deck.Cards.Add(card, 3);
            m_deck.Cards.Add(sameCardInSpecificSet, 1);

            Assert.AreEqual(3, m_deck.Cards["Hello"]);
        }

        [Test]
        public void Test_Cards_that_are_not_in_the_deck_return_0_occurences()
        {
            CardIdentifier card = new CardIdentifier { Card = "Hello" };
            Assert.AreEqual(0, m_deck.Cards[card]);
        }

        [Test]
        public void Test_Cannot_add_an_invalid_card()
        {
            Assert.Throws<ArgumentException>(() => m_deck.Cards.Add(new CardIdentifier()));
            Assert.Throws<ArgumentException>(() => m_deck.Cards.Remove(new CardIdentifier()));
            Assert.Throws<ArgumentException>(() => m_deck.Cards[new CardIdentifier()] = 2);
        }

        [Test]
        public void Test_Cannot_set_a_negative_count()
        {
            Assert.Throws<ArgumentException>(() => m_deck.Cards[new CardIdentifier { Card = "MyCard" }] = -1);
        }

        [Test]
        public void Test_Can_get_set_Name()
        {
            m_deck.Name = "My Deck";
            Assert.AreEqual("My Deck", m_deck.Name);
        }

        [Test]
        public void Test_Can_get_set_Author()
        {
            m_deck.Author = "Me";
            Assert.AreEqual("Me", m_deck.Author);
        }

        [Test]
        public void Test_Can_get_set_Description()
        {
            m_deck.Description = "My Description";
            Assert.AreEqual("My Description", m_deck.Description);
        }

        [Test]
        public void Test_Can_get_CreationTime()
        {
            Assert.IsNotNull(m_deck.CreationTime);
        }

        [Test]
        public void Test_By_default_the_LastModificationTime_is_the_CreationTime()
        {
            Assert.AreEqual(m_deck.CreationTime, m_deck.LastModificationTime);
        }

        [Test]
        public void Test_Can_get_set_the_LastModificationTime()
        {
            DateTime now = DateTime.Now;
            m_deck.LastModificationTime = now;
            Assert.AreEqual(now, m_deck.LastModificationTime);
        }

        #region Persistence

        private static Deck Persist(Deck deck)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    CloseOutput = false,
                    Indent = true
                };

                using (var writer = XmlWriter.Create(stream, settings))
                {
                    deck.Save(writer);
                }

                stream.Seek(0, SeekOrigin.Begin);

                Deck newDeck = new Deck();

                XPathDocument document = new XPathDocument(stream);
                newDeck.Load(document.CreateNavigator(), deck.Guid);
                return newDeck;
            }
        }

        [Test]
        public void Test_Decks_can_be_persisted()
        {
            m_deck.Name = "My Deck";
            m_deck.Author = "My Author";
            m_deck.LastModificationTime = DateTime.Now;

            CardIdentifier card1 = new CardIdentifier { Card = "My Card" };
            CardIdentifier card2 = new CardIdentifier { Card = "My Other Card", Set = "My Set" };
            m_deck.Cards[card1] = 3;
            m_deck.Cards[card2] = 1;

            Deck persistedDeck = Persist(m_deck);

            Assert.AreEqual(m_deck.Guid, persistedDeck.Guid);
            Assert.AreEqual(m_deck.Name, persistedDeck.Name);
            Assert.AreEqual(m_deck.Author, persistedDeck.Author);
            Assert.AreEqual(m_deck.CreationTime, persistedDeck.CreationTime);
            Assert.AreEqual(m_deck.LastModificationTime, persistedDeck.LastModificationTime);

            Assert.Collections.AreEquivalent(new[] { card1, card1, card1, card2 }, persistedDeck.Cards);
        }

        [Test]
        public void Test_Decks_can_be_persisted_with_weird_characters()
        {
            m_deck.Name = "My Deck\"";
            m_deck.Author = "My Author'";

            CardIdentifier card1 = new CardIdentifier { Card = "My Card\"\"" };
            CardIdentifier card2 = new CardIdentifier { Card = "My Other Card", Set = "My Set<" };
            m_deck.Cards[card1] = 3;
            m_deck.Cards[card2] = 1;

            Deck persistedDeck = Persist(m_deck);

            Assert.AreEqual(m_deck.Name, persistedDeck.Name);
            Assert.AreEqual(m_deck.Author, persistedDeck.Author);
            Assert.AreEqual(m_deck.CreationTime, persistedDeck.CreationTime);
            Assert.AreEqual(m_deck.LastModificationTime, persistedDeck.LastModificationTime);

            Assert.Collections.AreEquivalent(new[] { card1, card1, card1, card2 }, persistedDeck.Cards);
        }

        #endregion

        #endregion
    }
}
