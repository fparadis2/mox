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
    /// <summary>
    /// A database of cards/sets info.
    /// </summary>
    public class CardDatabase
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

        private readonly CardInfoCollection m_cards = new CardInfoCollection();
        private readonly SetInfoCollection m_sets = new SetInfoCollection();

        private readonly Dictionary<CardInfo, ICollection<CardInstanceInfo>> m_cardInstances = new Dictionary<CardInfo, ICollection<CardInstanceInfo>>();
        private readonly Dictionary<SetInfo, ICollection<CardInstanceInfo>> m_cardInstancesBySet = new Dictionary<SetInfo, ICollection<CardInstanceInfo>>();

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

        #endregion

        #region Methods

        #region Queries

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

        public CardInfo AddCard(string name, string manaCost, SuperType superType, Type type, IEnumerable<SubType> subTypes, string power, string toughness, string text)
        {
            CardInfo card = new CardInfo(this, name, manaCost, superType, type, subTypes, power, toughness, text);
            m_cards.Add(card);
            return card;
        }

        public SetInfo AddSet(string identifier, string name, string block, DateTime releaseDate)
        {
            SetInfo set = new SetInfo(this, identifier, name, block, releaseDate);
            m_sets.Add(set);
            return set;
        }

        public CardInstanceInfo AddCardInstance(CardInfo card, SetInfo set, Rarity rarity, int index, string artist)
        {
            CardInstanceInfo instance = new CardInstanceInfo(card, set, rarity, index, artist);

            AddCardInstance(m_cardInstances, card, instance);
            AddCardInstance(m_cardInstancesBySet, set, instance);

            return instance;
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
