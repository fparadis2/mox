using System;
using Mox.Database;
using NUnit.Framework;

namespace Mox.UI.Browser
{
    public class DeckViewModelTestsBase
    {
        #region Variables

        protected CardIdentifier m_card1;
        protected CardIdentifier m_card2;

        protected Deck m_deck;
        protected MockDeckViewModelEditor m_editor;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public virtual void Setup()
        {
            CardDatabase database = new CardDatabase();
            var set1 = database.AddSet("THESET", "My other set", "Block", DateTime.Now);
            var set2 = database.AddSet("My Set", "My super set", "Block", DateTime.Now);
            var card1 = database.AddCard("My Card", "R", SuperType.None, Type.Creature, new SubType[0], "1", "1", new string[0]);
            var card2 = database.AddCard("My Other Card", "R", SuperType.None, Type.Creature, new SubType[0], "1", "1", new string[0]);

            database.AddCardInstance(card1, set1, Rarity.Common, 1, "Papa john");
            database.AddCardInstance(card2, set1, Rarity.Common, 2, "Papa john");
            database.AddCardInstance(card2, set2, Rarity.Common, 1, "Papa john");

            m_card1 = new CardIdentifier { Card = card1.Name };
            m_card2 = new CardIdentifier { Card = card2.Name, Set = set2.Name };

            m_deck = new Deck
            {
                Name = "My Super Deck",
                Author = "Frank"
            };
            m_deck.Cards[m_card1] = 3;
            m_deck.Cards[m_card2] = 1;

            m_editor = new MockDeckViewModelEditor(database);
        }

        #endregion
    }
}