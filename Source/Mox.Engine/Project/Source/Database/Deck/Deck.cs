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
using System.Collections.Generic;

namespace Mox.Database
{
    /// <summary>
    /// IDeck is considered immutable. Once you get a hold on an IDeck, it cannot change.
    /// </summary>
    public interface IDeck
    {
        string Name { get; }
        string Description { get; }
        IReadOnlyList<CardIdentifier> Cards { get; }
        IReadOnlyList<CardIdentifier> Sideboard { get; }

        string Contents { get; }
        string Error { get; }
    }

    public class Deck : IDeck
    {
        #region Variables

        private readonly string m_name;
        private readonly CardCollection m_cards = new CardCollection();
        private readonly CardCollection m_sideboard = new CardCollection();

        #endregion

        #region Constructor

        public Deck(string name)
        {
            Throw.IfEmpty(name, "name");
            m_name = name;
        }

        #endregion

        #region Properties

        public string Contents
        {
            get; set;
        }

        public CardCollection Cards
        {
            get { return m_cards; }
        }

        IReadOnlyList<CardIdentifier> IDeck.Cards
        {
            get { return m_cards; }
        }

        public CardCollection Sideboard
        {
            get { return m_sideboard; }
        }

        IReadOnlyList<CardIdentifier> IDeck.Sideboard
        {
            get { return m_sideboard; }
        }
        
        public string Name
        {
            get { return m_name; }
        }

        public string Description
        {
            get; set;
        }

        public string Error
        {
            get;
            set;
        }

        #endregion

        #region Methods

        public static Deck Read(string name, string contents)
        {
            DeckReader reader = new DeckReader(name);
            return reader.Read(contents);
        }

        #endregion

        #region Inner Types

        public class CardCollection : List<CardIdentifier>
        {
            public void Add(string cardName, int quantity)
            {
                CardIdentifier card = new CardIdentifier { Card = cardName };
                Add(card, quantity);
            }

            public void Add(CardIdentifier card, int quantity)
            {
                for (int i = 0; i < quantity; i++)
                {
                    Add(card);
                }
            }
        }

        #endregion
    }
}