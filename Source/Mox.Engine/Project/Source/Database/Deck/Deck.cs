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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;

namespace Mox.Database
{
    /// <summary>
    /// A deck is a collection of card infos.
    /// </summary>
    public class Deck
    {
        #region Inner Types

        public class CardCollection : IEnumerable<CardIdentifier>
        {
            #region Variables

            private readonly Dictionary<CardIdentifier, int> m_cards = new Dictionary<CardIdentifier, int>();

            #endregion

            #region Properties

            public int this[CardIdentifier card]
            {
                get
                {
                    int result;
                    m_cards.TryGetValue(card, out result);
                    Debug.Assert(result >= 0);
                    return result;
                }
                set
                {
                    Throw.InvalidArgumentIf(value < 0, "Invalid count", "Deck[CardIdentifier]");
                    ValidateCard(card);
                    SetCount(card, value);
                }
            }

            public int this[string cardName]
            {
                get { return this[new CardIdentifier { Card = cardName }]; }
                set { this[new CardIdentifier { Card = cardName }] = value; }
            }

            public ICollection<CardIdentifier> Keys
            {
                get { return m_cards.Keys; }
            }

            #endregion

            #region Methods

            public void Add(CardIdentifier card)
            {
                Add(card, 1);
            }

            public void Add(CardIdentifier card, int times)
            {
                ValidateCard(card);

                int newCount = this[card] + times;
                SetCount(card, newCount);
            }

            public void Add(string card, int times)
            {
                Add(new CardIdentifier { Card = card }, times);
            }

            public void Remove(CardIdentifier card)
            {
                Remove(card, 1);
            }

            public void Remove(CardIdentifier card, int times)
            {
                ValidateCard(card);

                int newCount = this[card] - times;
                newCount = Math.Max(0, newCount);
                SetCount(card, newCount);
            }

            public bool ContainsKey(CardIdentifier card)
            {
                ValidateCard(card);
                return m_cards.ContainsKey(card);
            }

            private static void ValidateCard(CardIdentifier card)
            {
                Throw.InvalidArgumentIf(card.IsInvalid, "Invalid card", "card");
            }

            private void SetCount(CardIdentifier card, int count)
            {
                if (count == 0)
                {
                    m_cards.Remove(card);
                }
                else
                {
                    m_cards[card] = count;
                }
            }

            public IEnumerator<CardIdentifier> GetEnumerator()
            {
                return EnumerableImpl().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private IEnumerable<CardIdentifier> EnumerableImpl()
            {
                foreach (var pair in m_cards)
                {
                    for (int i = 0; i < pair.Value; i++)
                    {
                        yield return pair.Key;
                    }
                }
            }

            internal void Save(XmlWriter writer)
            {
                foreach (var pair in m_cards)
                {
                    writer.WriteStartElement(XmlConstants.CardElement);
                    {
                        writer.WriteAttributeString(XmlConstants.CardNameAttribute, pair.Key.Card);

                        if (!string.IsNullOrEmpty(pair.Key.Set))
                        {
                            writer.WriteAttributeString(XmlConstants.CardSetAttribute, pair.Key.Set);
                        }

                        writer.WriteAttributeString(XmlConstants.CardCountAttribute, pair.Value.ToString());
                    }
                    writer.WriteEndElement();
                }
            }

            internal void Load(XPathNavigator navigator)
            {
                m_cards.Clear();

                foreach (XPathNavigator cardNavigator in navigator.Select(XmlConstants.CardElement))
                {
                    CardIdentifier identifier = new CardIdentifier
                    {
                        Card = cardNavigator.GetAttributeValue(XmlConstants.CardNameAttribute),
                        Set = cardNavigator.GetAttributeValue(XmlConstants.CardSetAttribute)
                    };
                    int count = cardNavigator.GetAttributeValue(XmlConstants.CardCountAttribute, int.Parse);
                    m_cards[identifier] = count;
                }
            }

            #endregion
        }

        #endregion

        #region Variables

        private Guid m_guid = Guid.NewGuid();
        private readonly CardCollection m_cards = new CardCollection();

        #endregion

        #region Constructor

        public Deck()
        {
            LastModificationTime = CreationTime = DateTime.Now;
        }

        #endregion

        #region Properties

        public Guid Guid
        {
            get { return m_guid; }
        }

        /// <summary>
        /// Cards in the deck.
        /// </summary>
        public CardCollection Cards
        {
            get { return m_cards; }
        }

        /// <summary>
        /// Name of the deck.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Author of the deck.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Description of the deck.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Creation Time.
        /// </summary>
        public DateTime CreationTime
        {
            get;
            private set;
        }

        /// <summary>
        /// Last Modification Time.
        /// </summary>
        public DateTime LastModificationTime { get; set; }

        #endregion

        #region Methods

        #region Persistence

        private static class XmlConstants
        {
            public const string RootElement = "Deck";

            public const string NameAttribute = "name";
            public const string AuthorAttribute = "author";
            public const string DescriptionAttribute = "description";
            public const string CreationTimeAttribute = "ctime";
            public const string LastModificationTimeAttribute = "mtime";

            public const string CardsElement = "Cards";
            public const string CardElement = "Card";
            public const string CardNameAttribute = "name";
            public const string CardSetAttribute = "set";
            public const string CardCountAttribute = "count";
        }

        internal void Save(XmlWriter writer)
        {
            writer.WriteStartElement(XmlConstants.RootElement);
            {
                writer.WriteAttributeString(XmlConstants.NameAttribute, Name);
                writer.WriteAttributeString(XmlConstants.AuthorAttribute, Author);
                writer.WriteAttributeString(XmlConstants.DescriptionAttribute, Description);
                writer.WriteAttributeString(XmlConstants.CreationTimeAttribute, CreationTime.ToString("o"));
                writer.WriteAttributeString(XmlConstants.LastModificationTimeAttribute, LastModificationTime.ToString("o"));

                writer.WriteStartElement(XmlConstants.CardsElement);
                {
                    m_cards.Save(writer);
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        internal void Load(XPathNavigator navigator, Guid guid)
        {
            m_guid = guid;

            navigator = navigator.SelectSingleNode(XmlConstants.RootElement);

            Name = navigator.GetAttributeValue(XmlConstants.NameAttribute);
            Author = navigator.GetAttributeValue(XmlConstants.AuthorAttribute);
            Description = navigator.GetAttributeValue(XmlConstants.DescriptionAttribute);
            CreationTime = navigator.GetAttributeValue(XmlConstants.CreationTimeAttribute, DateTime.Parse);
            LastModificationTime = navigator.GetAttributeValue(XmlConstants.LastModificationTimeAttribute, DateTime.Parse);

            m_cards.Load(navigator.SelectSingleNode(XmlConstants.CardsElement));
        }

        #endregion

        #endregion
    }
}