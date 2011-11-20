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
using System.IO;
using System.Linq;
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

            private readonly Dictionary<string, DeckCard> m_cards = new Dictionary<string, DeckCard>();

            #endregion

            #region Properties

            public int this[CardIdentifier card]
            {
                get
                {
                    DeckCard result;
                    if (m_cards.TryGetValue(card.Card, out result))
                    {
                        Debug.Assert(result.Quantity >= 0);
                        return result.Quantity;
                    }
                    return 0;
                }
                set
                {
                    SetCount(card, value);
                    UseSet(card);
                }
            }

            public int this[string cardName]
            {
                get { return this[new CardIdentifier { Card = cardName }]; }
                set { SetCount(new CardIdentifier { Card = cardName }, value); }
            }

            public IEnumerable<CardIdentifier> Keys
            {
                get { return m_cards.Values.Select(card => card.CardIdentifier); }
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

            public void Clear()
            {
                m_cards.Clear();
            }

            public bool ContainsKey(CardIdentifier card)
            {
                ValidateCard(card);
                return m_cards.ContainsKey(card.Card);
            }

            public void UseSet(CardIdentifier card)
            {
                ValidateCard(card);

                DeckCard existing;
                if (m_cards.TryGetValue(card.Card, out existing))
                {
                    existing.CardIdentifier = card;
                }
            }

            private static void ValidateCard(CardIdentifier card)
            {
                Throw.InvalidArgumentIf(card.IsInvalid, "Invalid card", "card");
            }

            private void SetCount(CardIdentifier card, int count)
            {
                Throw.InvalidArgumentIf(count < 0, "Invalid count", "Card count");
                ValidateCard(card);

                if (count == 0)
                {
                    m_cards.Remove(card.Card);
                }
                else
                {
                    DeckCard existing;
                    if (!m_cards.TryGetValue(card.Card, out existing))
                    {
                        existing = new DeckCard(card);
                        m_cards.Add(card.Card, existing);
                    }
                    existing.Quantity = count;
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
                foreach (var card in m_cards.Values)
                {
                    for (int i = 0; i < card.Quantity; i++)
                    {
                        yield return card.CardIdentifier;
                    }
                }
            }

            internal void Save(XmlWriter writer)
            {
                foreach (var card in m_cards.Values)
                {
                    writer.WriteStartElement(XmlConstants.CardElement);
                    {
                        writer.WriteAttributeString(XmlConstants.CardNameAttribute, card.CardIdentifier.Card);

                        if (!string.IsNullOrEmpty(card.CardIdentifier.Set))
                        {
                            writer.WriteAttributeString(XmlConstants.CardSetAttribute, card.CardIdentifier.Set);
                        }

                        writer.WriteAttributeString(XmlConstants.CardCountAttribute, card.Quantity.ToString());
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
                    this[identifier] = count;
                }
            }

            #endregion

            #region Inner Types

            private class DeckCard
            {
                private CardIdentifier m_cardIdentifier;

                public DeckCard(CardIdentifier identifier)
                {
                    m_cardIdentifier = identifier;
                }

                public CardIdentifier CardIdentifier
                {
                    get { return m_cardIdentifier; }
                    set
                    {
                        Debug.Assert(CardIdentifier.Card == value.Card, "Cannot change card name");
                        m_cardIdentifier = value;
                    }
                }

                public int Quantity
                {
                    get;
                    set;
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

#warning [MEDIUM] Update on saving the deck

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

        internal void Save(Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true
            };

            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                Save(writer);
            }
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

        internal static Deck Load(Stream stream, Guid guid)
        {
            Deck deck = new Deck();
            XPathDocument document = new XPathDocument(stream);
            deck.Load(document.CreateNavigator(), guid);
            return deck;
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

        public Deck Clone()
        {
            // Use serialization for now (good programmers are lazy!)
            using (MemoryStream stream = new MemoryStream())
            {
                Save(stream);

                stream.Seek(0, SeekOrigin.Begin);

                return Load(stream, Guid);
            }
        }
    }
}