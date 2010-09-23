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

namespace Mox.Collections
{
    public class ReadOnlyKeyedCollection<TKey, TValue> : ReadOnlyListBase<TValue>, IKeyedList<TKey, TValue>
    {
        private readonly KeyedCollection<TKey, TValue> m_collection;

        public ReadOnlyKeyedCollection(KeyedCollection<TKey, TValue> collection)
        {
            m_collection = collection;
        }

        public override bool Contains(TValue item)
        {
            return m_collection.Contains(item);
        }

        public override IEnumerator<TValue> GetEnumerator()
        {
            return m_collection.GetEnumerator();
        }

        public override int Count
        {
            get { return m_collection.Count; }
        }

        public TValue this[TKey key]
        {
            get { return m_collection[key]; }
        }

        public bool ContainsKey(TKey key)
        {
            return m_collection.Contains(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (m_collection.Contains(key))
            {
                value = m_collection[key];
                return true;
            }

            value = default(TValue);
            return false;
        }

        public override int IndexOf(TValue item)
        {
            return m_collection.IndexOf(item);
        }

        public override TValue this[int index]
        {
            get { return m_collection[index]; }
            set { ReadOnlyException(); }
        }
    }
}