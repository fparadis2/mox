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
using NUnit.Framework;

namespace Mox.UI.Browser
{
    public class DeckViewModelTestsBase
    {
        #region Variables

        protected CardIdentifier m_card1;
        protected CardIdentifier m_card2;

        protected DeckViewModelEditor m_editor;
        protected DeckViewModel m_deckViewModel;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public virtual void Setup()
        {
            CardDatabase database = new CardDatabase();
            var set1 = database.AddSet("THESET", "My other set", "Block", DateTime.Now);
            var set2 = database.AddSet("My Set", "My super set", "Block", DateTime.Now);
            var card1 = database.AddCard("My Card", "R", SuperType.None, Type.Creature, new SubType[0], "1", "1", null);
            var card2 = database.AddCard("My Other Card", "R", SuperType.None, Type.Creature, new SubType[0], "1", "1", null);

            database.AddCardInstance(card1, set1, Rarity.Common, 1, "Papa john");
            database.AddCardInstance(card1, set2, Rarity.MythicRare, 1, "Papa john");
            database.AddCardInstance(card2, set1, Rarity.Common, 2, "Papa john");
            database.AddCardInstance(card2, set2, Rarity.Common, 2, "Papa john");

            m_card1 = new CardIdentifier { Card = card1.Name };
            m_card2 = new CardIdentifier { Card = card2.Name, Set = set2.Name };

            var initialDeck = new Deck
            {
                Name = "My Super Deck",
                Author = "Frank"
            };
            initialDeck.Cards[m_card1] = 2;
            initialDeck.Cards[m_card2] = 1;

            m_editor = new DeckViewModelEditor(database, null);
            m_deckViewModel = new DeckViewModel(initialDeck, m_editor);
            m_deckViewModel.Cards.ToString(); // Force creation of cards
        }

        #endregion

        #region Utilities

        protected Deck Deck
        {
            get { return m_deckViewModel.Deck; }
        }

        protected void Assert_SetsDirty(System.Action action)
        {
            m_deckViewModel.IsDirty = false;
            action();
            Assert.That(m_deckViewModel.IsDirty);
        }

        #endregion
    }
}