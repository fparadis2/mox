using System;
using System.Linq;
using NUnit.Framework;

namespace Mox.Database
{
    [TestFixture]
    public class DeckReaderTests
    {
        #region Utilities

        protected Deck ReadDeck(string contents)
        {
            Deck deck = Deck.Read("My Deck", contents);
            Assert.IsNotNull(deck);
            Assert.IsNullOrEmpty(deck.Error);
            return deck;
        }

        protected Deck ReadDeckWithError(string contents, string error)
        {
            Deck deck = Deck.Read("My Deck", contents);
            Assert.IsNotNull(deck);
            Assert.AreEqual(error, deck.Error);
            return deck;
        }

        #endregion

        #region Methods

        [Test]
        public void Test_Can_read_a_null_string()
        {
            foreach (var nullString in new [] { null, string.Empty })
            {
                var deck = Deck.Read("My Deck", nullString);
                Assert.AreEqual("My Deck", deck.Name);
                Assert.IsNullOrEmpty(deck.Description);
                Assert.IsNullOrEmpty(deck.Error);
                Assert.Collections.IsEmpty(deck.Cards);
                Assert.Collections.IsEmpty(deck.Sideboard);
            }
        }

        [Test]
        public void Test_Can_import_deck_with_different_indentations()
        {
            const string DeckString = @"
5 Forest
  2 Island
    2 Plains
4 Birds of Paradise

";

            Deck deck = ReadDeck(DeckString);

            Assert.IsNullOrEmpty(deck.Description);
            Assert.Collections.IsEmpty(deck.Sideboard);

            Assert.AreEqual(13, deck.Cards.Count);
            Assert.AreEqual(5, deck.Cards.Count(c => c.Card == "Forest"));
            Assert.AreEqual(2, deck.Cards.Count(c => c.Card == "Island"));
            Assert.AreEqual(2, deck.Cards.Count(c => c.Card == "Plains"));
            Assert.AreEqual(4, deck.Cards.Count(c => c.Card == "Birds of Paradise"));
        }

        [Test]
        public void Test_First_comment_lines_are_interpreted_as_the_description()
        {
            const string DeckString = @"
// Some super comment
// about the deck
5 Forest
";

            Deck deck = ReadDeck(DeckString);

            Assert.AreEqual(5, deck.Cards.Count);
            Assert.That(deck.Cards.All(c => c.Card == "Forest"));

            Assert.AreEqual("Some super comment" + Environment.NewLine + "about the deck", deck.Description);
        }

        [Test]
        public void Test_MTGO_style_sideboard_is_correctly_read()
        {
            const string DeckString = @"
5 Forest

2 Jace's Ingenuity
2 Obstinate Baloth
4 Celestial Purge
";

            Deck deck = ReadDeck(DeckString);

            Assert.IsNullOrEmpty(deck.Description);

            Assert.AreEqual(5, deck.Cards.Count);
            Assert.That(deck.Cards.All(c => c.Card == "Forest"));

            Assert.AreEqual(8, deck.Sideboard.Count);
            Assert.AreEqual(2, deck.Sideboard.Count(c => c.Card == "Jace's Ingenuity"));
            Assert.AreEqual(2, deck.Sideboard.Count(c => c.Card == "Obstinate Baloth"));
            Assert.AreEqual(4, deck.Sideboard.Count(c => c.Card == "Celestial Purge"));
        }

        [Test]
        public void Test_Apprentice_style_sideboard_is_correctly_read()
        {
            const string DeckString = @"
5 Forest

// Sideboard:
SB: 2 Jace's Ingenuity
SB: 2 Obstinate Baloth
SB: 4 Celestial Purge
";

            Deck deck = ReadDeck(DeckString);

            Assert.IsNullOrEmpty(deck.Description);

            Assert.AreEqual(5, deck.Cards.Count);
            Assert.That(deck.Cards.All(c => c.Card == "Forest"));

            Assert.AreEqual(8, deck.Sideboard.Count);
            Assert.AreEqual(2, deck.Sideboard.Count(c => c.Card == "Jace's Ingenuity"));
            Assert.AreEqual(2, deck.Sideboard.Count(c => c.Card == "Obstinate Baloth"));
            Assert.AreEqual(4, deck.Sideboard.Count(c => c.Card == "Celestial Purge"));
        }

        [Test]
        public void Test_Cannot_import_a_deck_with_ill_formed_lines_1()
        {
            const string DeckString = @"
Forest asdas asd
2 Island
2 Plains
4 Birds of Paradise
";

            ReadDeckWithError(DeckString, "'Forest' is not a valid card quantity");
        }

        [Test]
        public void Test_Cannot_import_a_deck_with_ill_formed_lines_2()
        {
            const string DeckString = @"
Forest 5
2 Island
2 Plains
4 Birds of Paradise
";

            ReadDeckWithError(DeckString, "'Forest' is not a valid card quantity");
        }

        [Test]
        public void Test_Cannot_import_a_deck_with_ill_formed_lines_3()
        {
            const string DeckString = @"
5
2 Island
2 Plains
4 Birds of Paradise
";

            ReadDeckWithError(DeckString, "'5' is an invalid line");
        }

        [Test]
        public void Test_Quantity_must_be_positive()
        {
            const string DeckString = @"
0 Forest
2 Island
2 Forest
4 Birds of Paradise
";

            ReadDeckWithError(DeckString, "'0' is not a valid card quantity");
        }

        #endregion
    }
}
