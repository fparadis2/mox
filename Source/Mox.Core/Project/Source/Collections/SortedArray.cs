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
using System.Linq;
using System.Text;

namespace Mox.Collections
{
    public class SortedArray<T> : IList<T>
    {
        #region Constants

        private const int InitialCapacity = 3;
        private const double GrowFactor = 1.2;

        #endregion

        #region Variables

        private T[] m_store;
        private int m_effectiveValueCount;
        private readonly IComparer<T> m_comparer;

        #endregion

        #region Constructor

        public SortedArray()
            : this(Comparer<T>.Default)
        {
        }

        public SortedArray(IComparer<T> comparer)
        {
            Throw.IfNull(comparer, "comparer");
            m_comparer = comparer;
        }

        #endregion

        #region Properties

        public IComparer<T> Comparer
        {
            get
            {
                return m_comparer;
            }
        }

        #endregion

        #region Methods

        private void InsertValue(T value, int index)
        {
            Debug.Assert(index >= 0 && index <= m_effectiveValueCount);

            int effectiveValuesCount = m_effectiveValueCount;
            if (effectiveValuesCount > 0)
            {
                if (m_store.Length == effectiveValuesCount)
                {
                    int newLength = (int)(effectiveValuesCount * GrowFactor);
                    if (newLength == effectiveValuesCount)
                    {
                        newLength++;
                    }
                    T[] destinationArray = new T[newLength];
                    Array.Copy(m_store, 0, destinationArray, 0, index);
                    destinationArray[index] = value;
                    Array.Copy(m_store, index, destinationArray, index + 1, effectiveValuesCount - index);
                    m_store = destinationArray;
                }
                else
                {
                    Array.Copy(m_store, index, m_store, index + 1, effectiveValuesCount - index);
                    m_store[index] = value;
                }
            }
            else
            {
                Debug.Assert(index == 0);
                if (m_store == null)
                {
                    m_store = new T[InitialCapacity];
                }
                m_store[0] = value;
            }

            m_effectiveValueCount = effectiveValuesCount + 1;
        }

        private void RemoveValue(int index)
        {
            Debug.Assert(index >= 0 && index < m_effectiveValueCount);

            Array.Copy(m_store, index + 1, m_store, index, m_effectiveValueCount - index - 1);
            m_effectiveValueCount--;
        }

        private int LookupValue(T value, SearchDirection direction)
        {
            if (m_effectiveValueCount == 0)
            {
                return 0;
            }

            return BinarySearch(value, direction);
        }

        private int BinarySearch(T value, SearchDirection direction)
        {
            int min = 0;
            int max = m_effectiveValueCount - 1;
            while (min <= max)
            {
                int mid = min + ((max - min) >> 1);
                int compareResult = m_comparer.Compare(m_store[mid], value);

                if (compareResult == 0)
                {
                    int sign;

                    switch (direction)
                    {
                        case SearchDirection.None:
                            return mid;

                        case SearchDirection.Max:
                            sign = +1;
                            break;

                        case SearchDirection.Min:
                            sign = -1;
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    return LinearSearch(value, mid, sign);
                }

                if (compareResult < 0)
                {
                    min = mid + 1;
                }
                else
                {
                    max = mid - 1;
                }
            }
            return ~min;
        }

        private int LinearSearch(T value, int index, int sign)
        {
            Debug.Assert(sign == 1 || sign == -1);

            do
            {
                index += sign;
            }
            while (IsValidIndex(value, index));

            if (sign < 0)
            {
                index -= sign;
            }

            return index;
        }

        private bool IsValidIndex(T value, int index)
        {
            return index < m_effectiveValueCount && index >= 0 && m_comparer.Compare(m_store[index], value) == 0;
        }

        private IEnumerable<int> GetValuesWithSameKey(T value)
        {
            for (int index = LookupValue(value, SearchDirection.Min); IsValidIndex(value, index); index++)
            {
                yield return index;
            }
        }

        private enum SearchDirection
        {
            None,
            Min,
            Max
        }

        #endregion

        #region Implementations

        #region Implementation of IEnumerable

        private IEnumerable<T> Enumerate()
        {
            for (int i = 0; i < m_effectiveValueCount; i++)
            {
                yield return m_store[i];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<T>

        public void Add(T item)
        {
            int index = LookupValue(item, SearchDirection.Max);

            if (index < 0)
            {
                index = ~index;
            }

            InsertValue(item, index);
        }

        public bool Remove(T item)
        {
            foreach (int index in GetValuesWithSameKey(item))
            {
                if (Equals(item, m_store[index]))
                {
                    RemoveValue(index);
                    return true;
                }
            }
            
            return false;
        }

        public void Clear()
        {
            m_store = null;
            m_effectiveValueCount = 0;
        }

        public bool Contains(T item)
        {
            foreach (int index in GetValuesWithSameKey(item))
            {
                if (Equals(item, m_store[index]))
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (m_effectiveValueCount > 0)
            {
                Array.Copy(m_store, 0, array, arrayIndex, m_effectiveValueCount);
            }
        }

        public int Count
        {
            get { return m_effectiveValueCount; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region Implementation of IList<T>

        public int IndexOf(T item)
        {
            return LookupValue(item, SearchDirection.Min);
        }

        public void Insert(int index, T item)
        {
            throw new InvalidOperationException();
        }

        public void RemoveAt(int index)
        {
            ValidateIndex(index);
            RemoveValue(index);
        }

        public T this[int index]
        {
            get 
            {
                ValidateIndex(index);
                return m_store[index];
            }
            set { throw new InvalidOperationException(); }
        }

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= m_effectiveValueCount)
            {
                throw new IndexOutOfRangeException("Index out of range");
            }
        }

        #endregion

        #endregion
    }
}
