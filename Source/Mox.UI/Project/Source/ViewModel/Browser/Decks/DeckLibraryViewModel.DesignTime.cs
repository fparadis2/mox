using System.Linq;
using Mox.Database;

namespace Mox.UI.Browser
{
    public class DesignTimeDeckLibraryViewModel : DeckLibraryViewModel
    {
        public DesignTimeDeckLibraryViewModel()
            : this(new EditDeckViewModel(DesignTimeCardDatabase.Instance), CreateLibrary())
        {
        }

        public DesignTimeDeckLibraryViewModel(IDeckViewModelEditor editor, DeckLibrary library)
            : base(editor, library)
        {
            SelectedDeck = Decks.First();
        }

        internal static DeckLibrary CreateLibrary()
        {
            DeckLibrary library = new DeckLibrary();

            CardIdentifier card1 = new CardIdentifier { Card = "Mousse" };
            CardIdentifier card2 = new CardIdentifier { Card = "Turned yogurt" };

            Deck deck1 = new Deck
            {
                Name = "My First Deck",
                Author = "Picasso",
                Description = "This is my first deck. I'm proud of it!"
            };

            deck1.Cards[card1] = 3;
            deck1.Cards[card2] = 1;
            library.Save(deck1);

            Deck deck2 = new Deck
            {
                Name = "My Second Deck",
                Author = "King Kong",
                Description = "This one is not so good."
            };

            deck2.Cards[card1] = 2;
            deck2.Cards[card2] = 2;
            library.Save(deck2);

            return library;
        }

        internal static Deck CreateDeck()
        {
            CardIdentifier card1 = new CardIdentifier { Card = "Mousse" };
            CardIdentifier card2 = new CardIdentifier { Card = "Turned yogurt" };

            Deck deck1 = new Deck
            {
                Name = "My First Deck",
                Author = "Picasso",
                Description = "This is my first deck. I'm proud of it!"
            };

            deck1.Cards[card1] = 3;
            deck1.Cards[card2] = 1;

            return deck1;
        }
    }
}