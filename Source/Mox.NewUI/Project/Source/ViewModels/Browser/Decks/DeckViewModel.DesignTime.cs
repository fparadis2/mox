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
using Mox.Database;

namespace Mox.UI.Browser
{
    public class DeckViewModel_DesignTime : DeckViewModel
    {
        public DeckViewModel_DesignTime()
            : base(CreateDeck(), CreateEditViewModel())
        {
        }

        private static IDeckViewModelEditor CreateEditViewModel()
        {
            var model = DeckViewModelEditor.FromDesignTime();
            model.IsEnabled = true;
            return model;
        }

        internal static Deck CreateDeck()
        {
            CardIdentifier card1 = new CardIdentifier { Card = "Mousse" };
            CardIdentifier card2 = new CardIdentifier { Card = "Turned yogurt" };

            Deck deck = new Deck
            {
                Name = "My First Deck",
                Author = "Picasso",
                Description = "This is my first deck. I'm proud of it! This deck is sponsored by Falcon inc." + Environment.NewLine + "copyright 1854"
            };

            deck.Cards[card1] = 3;
            deck.Cards[card2] = 1;

            return deck;
        }
    }
}