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

namespace Mox.UI
{
    /// <summary>
    /// Some hardcoded decks for testing
    /// </summary>
    public static class Decks
    {
        #region Cho-Manno's Resolve

        public static Deck Create_ChoMannosResolve()
        {
            Deck deck = new Deck();

            deck.Add(1, "Ghost Warden");
            deck.Add(2, "Youthful Knight");
            deck.Add(2, "Benalish Knight");
            deck.Add(1, "Venerable Monk");
            deck.Add(2, "Wild Griffin");
            deck.Add(1, "Cho-Manno, Revolutionary");    // TODO
            deck.Add(2, "Skyhunter Patrol");
            deck.Add(2, "Angel of Mercy");
            deck.Add(2, "Loxodon Mystic");
            deck.Add(1, "Ancestor's Chosen");
            deck.Add(1, "Condemn");
            deck.Add(2, "Pacifism");
            deck.Add(1, "Pariah");                      // TODO
            deck.Add(1, "Serra's Embrace");
            deck.Add(1, "Angel's Feather");
            deck.Add(1, "Icy Manipulator");

            deck.Add(17, "Plains");

            return deck;
        }

        public static Deck Create_Molimos_Might()
        {
            Deck deck = new Deck();

            deck.Add(2, "Canopy Spider");
            deck.Add(2, "Civic Wayfinder");          // TODO
            deck.Add(2, "Rootwalla");                // TODO
            deck.Add(1, "Stampeding Wildebeests");   // TODO
            deck.Add(2, "Spined Wurm");
            deck.Add(1, "Kavu Climber");
            deck.Add(1, "Craw Wurm");
            deck.Add(1, "Enormous Baloth");
            deck.Add(1, "Molimo, Maro-Sorcerer");    // TODO
            deck.Add(1, "Mantis Engine");
            deck.Add(2, "Commune with Nature");      // TODO
            deck.Add(1, "Giant Growth");
            deck.Add(2, "Rampant Growth");           // TODO
            deck.Add(2, "Blanchwood Armor");         // TODO
            deck.Add(1, "Overrun");                  // TODO
            deck.Add(1, "Hurricane");                // TODO
            deck.Add(1, "Wurm's Tooth");

            deck.Add(16, "Forest");

            return deck;
        }

        public static Deck Create_Kamahls_Temper()
        {
            Deck deck = new Deck();

            deck.Add(1, "Raging Goblin");
            deck.Add(1, "Viashino Sandscout");
            deck.Add(2, "Bloodrock Cyclops");        // TODO
            deck.Add(2, "Bogardan Firefiend");
            deck.Add(1, "Prodigal Pyromancer");
            deck.Add(2, "Lightning Elemental");
            deck.Add(1, "Furnace Whelp");
            deck.Add(2, "Thundering Giant");
            deck.Add(1, "Kamahl, Pit Fighter");
            deck.Add(1, "Shock");
            deck.Add(2, "Incinerate");               // TODO
            deck.Add(2, "Spitting Earth");
            deck.Add(1, "Threaten");
            deck.Add(1, "Beacon of Destruction");
            deck.Add(1, "Blaze");                    // TODO
            deck.Add(1, "Dragon's Claw");
            deck.Add(1, "Phyrexian Vault");

            deck.Add(17, "Mountain");

            return deck;
        }

        public static Deck Create_Test()
        {
            Deck deck = new Deck();

            deck.Add(8, "Mountain");
            deck.Add(7, "Shock");
            deck.Add(7, "Threaten");

            return deck;
        }

        private static void Add(this Deck deck, int times, string card)
        {
            deck.Cards.Add(new CardIdentifier { Card = card }, times);
        }

        #endregion
    }
}
