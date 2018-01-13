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
using System.Collections.ObjectModel;
using System.Linq;
using Mox.Collections;

namespace Mox.Database
{
    public interface ICardDatabase
    {
        ICardFactory Factory { get; }

        CardInfo GetCard(string name);
        CardIdentifier ResolveCardIdentifier(CardIdentifier card);
    }

    /// <summary>
    /// A database of cards/sets info.
    /// </summary>
    public class CardDatabase : ICardDatabase
    {
        #region Inner Types

        private class CardInfoCollection : KeyedCollection<string, CardInfo>
        {
            protected override string GetKeyForItem(CardInfo item)
            {
                return item.Name;
            }
        }

        private class SetInfoCollection : KeyedCollection<string, SetInfo>
        {
            protected override string GetKeyForItem(SetInfo item)
            {
                return item.Identifier;
            }
        }

        #endregion

        #region Variables

        private static readonly IRandom ms_random = Random.New();

        private readonly CardBuilder m_builder;
        private readonly CardInfoCollection m_cards = new CardInfoCollection();
        private readonly SetInfoCollection m_sets = new SetInfoCollection();

        private readonly Dictionary<CardInfo, ICollection<CardInstanceInfo>> m_cardInstances = new Dictionary<CardInfo, ICollection<CardInstanceInfo>>();
        private readonly Dictionary<SetInfo, ICollection<CardInstanceInfo>> m_cardInstancesBySet = new Dictionary<SetInfo, ICollection<CardInstanceInfo>>();
        private readonly Dictionary<int, CardInstanceInfo> m_cardInstancesByMultiverseId = new Dictionary<int, CardInstanceInfo>();

        #endregion

        #region Constructor

        public CardDatabase()
        {
            m_builder = new CardBuilder(this);
        }

        #endregion

        #region Properties

        public IKeyedList<string, CardInfo> Cards
        {
            get
            {
                return new ReadOnlyKeyedCollection<string, CardInfo>(m_cards);
            }
        }

        public IKeyedList<string, SetInfo> Sets
        {
            get
            {
                return new ReadOnlyKeyedCollection<string, SetInfo>(m_sets);
            }
        }

        public ICardFactory Factory
        {
            get { return m_builder; }
        }

        #endregion

        #region Methods

        #region Queries

        public CardInfo GetCard(string name)
        {
            Cards.TryGetValue(name, out CardInfo card);
            return card;
        }

        internal IEnumerable<CardInstanceInfo> GetCardInstances(CardInfo card)
        {
            return GetCardInstances(m_cardInstances, card);
        }

        internal IEnumerable<CardInstanceInfo> GetCardInstances(SetInfo set)
        {
            return GetCardInstances(m_cardInstancesBySet, set);
        }

        #endregion

        #region Add

        public CardInfo AddCard(string name, string manaCost, Color color, SuperType superType, Type type, IEnumerable<SubType> subTypes, string power, string toughness, string text)
        {
            CardInfo card = new CardInfo(this, name, manaCost, color, superType, type, subTypes, power, toughness, text);
            m_cards.Add(card);
            return card;
        }

        public SetInfo AddSet(string identifier, string name, string block, DateTime releaseDate)
        {
            SetInfo set = new SetInfo(this, identifier, name, block, releaseDate);
            m_sets.Add(set);
            return set;
        }

        public CardInstanceInfo AddCardInstance(CardInfo card, SetInfo set, int index, Rarity rarity, int multiverseId, string artist, string flavor = null)
        {
            CardInstanceInfo instance = new CardInstanceInfo(card, set, index, rarity, multiverseId, artist, flavor);

            AddCardInstance(m_cardInstances, card, instance);
            AddCardInstance(m_cardInstancesBySet, set, instance);

            if (multiverseId > 0)
                m_cardInstancesByMultiverseId.Add(multiverseId, instance);

            return instance;
        }

        public CardInstanceInfo GetCardInstance(CardIdentifier card)
        {
            CardInstanceInfo result;
            if (card.MultiverseId > 0 && m_cardInstancesByMultiverseId.TryGetValue(card.MultiverseId, out result))
                return result;

            Throw.InvalidArgumentIf(card.IsInvalid, "Invalid card", "card");
            CardInfo cardInfo;
            if (!m_cards.TryGetValue(card.Card, out cardInfo))
                return null;

            var potentialCards = cardInfo.Instances.Where(c => c.Set.Identifier == card.Set).ToList();

            if (potentialCards.Count == 0)
                potentialCards = cardInfo.Instances.ToList();

            return ms_random.Choose(potentialCards);
        }

        public CardIdentifier ResolveCardIdentifier(CardIdentifier card)
        {
            return GetCardInstance(card);
        }

        #region Utilities

        private static void AddCardInstance<TKey>(IDictionary<TKey, ICollection<CardInstanceInfo>> store, TKey key, CardInstanceInfo value)
        {
            ICollection<CardInstanceInfo> instances;
            if (!store.TryGetValue(key, out instances))
            {
                instances = new List<CardInstanceInfo>();
                store.Add(key, instances);
            }
            instances.Add(value);
        }

        private static IEnumerable<CardInstanceInfo> GetCardInstances<TKey>(IDictionary<TKey, ICollection<CardInstanceInfo>> store, TKey key)
        {
            ICollection<CardInstanceInfo> instances;
            if (store.TryGetValue(key, out instances))
            {
                return instances;
            }
            return Enumerable.Empty<CardInstanceInfo>();
        }

        #endregion

        #endregion

        #endregion
    }
}
