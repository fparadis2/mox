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
using System.Linq;
using Mox.Database;

namespace Mox.UI.Browser
{
    public class DeckLibraryViewModel_DesignTime : DeckLibraryViewModel
    {
        public DeckLibraryViewModel_DesignTime()
            : this(CreateLibrary(), DeckViewModelEditor.FromDesignTime())
        {
        }

        public DeckLibraryViewModel_DesignTime(DeckLibrary libraryViewModel, IDeckViewModelEditor editor)
            : base(libraryViewModel, editor)
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