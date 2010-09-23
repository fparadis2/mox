using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Mox.Database
{
    [TestFixture]
    public class DeckImporterTestsBase
    {
        #region Variables

        private CardDatabase m_database;

        private string m_lastMessage;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_database = new CardDatabase();

            m_database.AddDummyCard("Forest");
            m_database.AddDummyCard("Island");
            m_database.AddDummyCard("Plains");
            m_database.AddDummyCard("Birds of Paradise");
        }

        #endregion

        #region Utilities

        protected Deck Import(string deckString)
        {
            var deck = DeckImporter.Import(m_database, deckString, out m_lastMessage);

            using (Stream stream = deckString == null ? new MemoryStream() : new MemoryStream(Encoding.Default.GetBytes(deckString)))
            {
                string otherMsg;
                var otherDeck = DeckImporter.Import(m_database, stream, out otherMsg);
                Assert.AreEqual(m_lastMessage, otherMsg);
                Assert.AreEqual(deck != null, otherDeck != null);
            }

            return deck;
        }

        protected Deck Assert_Import(string deckString)
        {
            var result = Import(deckString);
            Assert.IsNotNull(result);
            Assert.IsNullOrEmpty(m_lastMessage);
            return result;
        }

        protected void Assert_Doesnt_Import(string deckString, string expectedMessage)
        {
            Assert.IsNull(Import(deckString));
            Assert.AreEqual(expectedMessage ?? string.Empty, m_lastMessage);
        }

        #endregion

        #region Methods

        [Test]
        public void Test_Invalid_construction_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => DeckImporter.Import(null, new MemoryStream(), out m_lastMessage));
            Assert.Throws<ArgumentNullException>(() => DeckImporter.Import(m_database, (Stream)null, out m_lastMessage));
        }

        [Test]
        public void Test_Cannot_import_a_null_or_empty_string()
        {
            Assert_Doesnt_Import(null, null);
            Assert_Doesnt_Import(string.Empty, null);
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

            Deck deck = Assert_Import(DeckString);

            Assert.AreEqual(4, deck.Cards.Keys.Count);
            Assert.AreEqual(5, deck.Cards["Forest"]);
            Assert.AreEqual(2, deck.Cards["Island"]);
            Assert.AreEqual(2, deck.Cards["Plains"]);
            Assert.AreEqual(4, deck.Cards["Birds of Paradise"]);
        }

        [Test]
        public void Test_First_lines_are_interpreted_as_name_and_description()
        {
            const string DeckString = @"
// Name of the deck
// Some super comment
// about the deck
5 Forest
";

            Deck deck = Assert_Import(DeckString);

            Assert.AreEqual(1, deck.Cards.Keys.Count);
            Assert.AreEqual(5, deck.Cards["Forest"]);

            Assert.AreEqual("Name of the deck", deck.Name);
            Assert.AreEqual("Some super comment" + Environment.NewLine + "about the deck", deck.Description);
        }

        [Test]
        public void Test_First_lines_can_contain_tags()
        {
            const string DeckString = @"
// NAME: Name of the deck
// CREATOR: Jesus
// Some super comment:
// UNKNOWNTAG: about the deck
// :not a tag
5 Forest
";

            Deck deck = Assert_Import(DeckString);

            Assert.AreEqual(1, deck.Cards.Keys.Count);
            Assert.AreEqual(5, deck.Cards["Forest"]);

            Assert.AreEqual("Name of the deck", deck.Name);
            Assert.AreEqual("Jesus", deck.Author);
            Assert.AreEqual("Some super comment:" + Environment.NewLine + "UNKNOWNTAG: about the deck" + Environment.NewLine + ":not a tag", deck.Description);
        }

        [Test]
        public void Test_Sideboard_is_ignored_for_now()
        {
            const string DeckString = @"
5 Forest

Sideboard
2 Jace's Ingenuity
2 Obstinate Baloth
4 Celestial Purge
";

            Deck deck = Assert_Import(DeckString);

            Assert.AreEqual(1, deck.Cards.Keys.Count);
            Assert.AreEqual(5, deck.Cards["Forest"]);
        }

        [Test]
        public void Test_Apprentice_style_sideboard_is_ignored_for_now()
        {
            const string DeckString = @"
5 Forest

// Sideboard:
SB: 2 Jace's Ingenuity
SB: 2 Obstinate Baloth
SB: 4 Celestial Purge
";

            Deck deck = Assert_Import(DeckString);

            Assert.AreEqual(1, deck.Cards.Keys.Count);
            Assert.AreEqual(5, deck.Cards["Forest"]);
        }

        [Test]
        public void Test_Cannot_import_a_deck_with_an_unknown_card()
        {
            const string DeckString = @"
5 Forest
2 Island
2 Plains2
4 Birds of Paradise
";

            Assert_Doesnt_Import(DeckString, "'Plains2' is not a known card");
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

            Assert_Doesnt_Import(DeckString, "'Forest' is not a valid card quantity");
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

            Assert_Doesnt_Import(DeckString, "'Forest' is not a valid card quantity");
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

            Assert_Doesnt_Import(DeckString, "'5' is an invalid line");
        }

        [Test]
        public void Test_Cards_cannot_appear_more_than_once()
        {
            const string DeckString = @"
5 Forest
2 Island
2 Forest
4 Birds of Paradise
";

            Assert_Doesnt_Import(DeckString, "Card 'Forest' appears more than once");
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

            Assert_Doesnt_Import(DeckString, "'0' is not a valid card quantity");
        }

        #endregion
    }
}
