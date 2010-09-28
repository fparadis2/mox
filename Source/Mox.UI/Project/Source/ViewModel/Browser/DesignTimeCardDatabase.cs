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
    public class DesignTimeCardDatabase : CardDatabase
    {
        private static readonly DesignTimeCardDatabase ms_instance = new DesignTimeCardDatabase();

        public static DesignTimeCardDatabase Instance
        {
            get { return ms_instance; }
        }

        private DesignTimeCardDatabase()
        {
            SetInfo coh = AddSet("M11", "Creatures of the house", "Joys of life", DateTime.Now);
            SetInfo fon = AddSet("TMP", "Forces of nature", "Joys of life", DateTime.Now.Subtract(TimeSpan.FromDays(10)));

            DesignTimeCardViewModel.CreateCardInfo(this, new[] { coh, fon });

            CardInfo mousse = AddCard("Mousse", "2UW", SuperType.Legendary, Type.Creature, new[] { SubType.Cat }, "8", "6", new[] { "Mousse is fearless" });
            CardInfo breeze = AddCard("The breeze of the matinee", "W", SuperType.None, Type.Enchantment, new SubType[0], "0", "0", new[] { "Feel the breeze!" });
            CardInfo longCard = AddCard("This card has a really really very long name and it's back with a vengeance", "W", SuperType.Basic | SuperType.Legendary, Type.Enchantment, new[] { SubType.Advisor, SubType.Ajani, SubType.Anteater, SubType.Archer, SubType.Assassin, SubType.Aura }, "0", "0", new[] { "Feel the breeze!" });

            AddCardInstance(mousse, coh, Rarity.Rare, 2, "Picasso");
            AddCardInstance(mousse, fon, Rarity.Uncommon, 2, "Michaelangelo");

            AddCardInstance(breeze, fon, Rarity.Common, 3, "Rembrandt");
            AddCardInstance(longCard, fon, Rarity.Common, 3, "Rembrandt");

            int index = 10;

            for (int i = 0; i < 10; i++)
            {
                GenerateRandomCard(coh, ref index);
            }
        }

        private static void GenerateRandomCard(SetInfo set, ref int generatedIndex)
        {
            string name = "Random card " + generatedIndex++;

            CardInfo cardInfo = set.Database.AddCard(name, "W", SuperType.None, Type.Creature, new[] { SubType.Gargoyle }, "2", "1", new[] { "This is random!" });
            set.Database.AddCardInstance(cardInfo, set, Rarity.Common, generatedIndex, "Pollock");
        }
    }
}
