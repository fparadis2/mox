using Mox.Database;

namespace Mox.UI.Browser
{
    public class DesignTimeDeckViewModel : DeckViewModel
    {
        public DesignTimeDeckViewModel()
            : base(new EditDeckViewModel(DesignTimeCardDatabase.Instance), CreateDeck())
        {
        }

        internal static Deck CreateDeck()
        {
            CardIdentifier card1 = new CardIdentifier { Card = "Mousse" };
            CardIdentifier card2 = new CardIdentifier { Card = "Turned yogurt" };

            Deck deck = new Deck
            {
                Name = "My First Deck",
                Author = "Picasso",
                Description = "This is my first deck. I'm proud of it!"
            };

            deck.Cards[card1] = 3;
            deck.Cards[card2] = 1;

            return deck;
        }
    }
}