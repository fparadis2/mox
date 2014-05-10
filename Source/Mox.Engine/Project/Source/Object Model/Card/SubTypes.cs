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
using System.Linq;
using System.Collections.Generic;

namespace Mox
{
    [Serializable]
    public class SubTypes : IHashable
    {
        #region Variables

        private static readonly SubTypes ms_empty;
        private static readonly int ms_length;

        private readonly ContiguousBitArray m_array;

        #endregion

        #region Constructor

        static SubTypes()
        {
            ms_length = Enum.GetValues(typeof(SubType)).Length;
            ms_empty = new SubTypes();
        }

        public SubTypes(params SubType[] subTypes)
            : this(subTypes.AsEnumerable())
        {
        }

        public SubTypes(IEnumerable<SubType> subTypes)
            : this()
        {
            foreach (SubType subType in subTypes)
            {
                m_array[(int)subType] = true;
            }
        }

        private SubTypes()
            : this(new ContiguousBitArray(ms_length))
        {
        }

        private SubTypes(ContiguousBitArray array)
        {
            m_array = array;
        }

        #endregion

        #region Properties

        public static SubTypes Empty
        {
            get { return ms_empty; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns true if the card is of the given <paramref name="subType"/>.
        /// </summary>
        /// <returns></returns>
        public bool Is(SubType subType)
        {
            return m_array[(int)subType];
        }

        /// <summary>
        /// Returns true if the card is of the given <paramref name="subTypes"/> (all of them).
        /// </summary>
        /// <returns></returns>
        public bool IsAll(params SubType[] subTypes)
        {
            return subTypes.All(Is);
        }

        /// <summary>
        /// Returns true if the card is of the given <paramref name="subTypes"/> (any of them).
        /// </summary>
        /// <returns></returns>
        public bool IsAny(params SubType[] subTypes)
        {
            return subTypes.Any(Is);
        }

        /// <summary>
        /// Returns the list of sub types contained in this instance.
        /// </summary>
        /// <remarks>
        /// This method is costly. For tests or debug only.
        /// </remarks>
        /// <returns></returns>
        public IEnumerable<SubType> ToList()
        {
            for (int i = 0; i < ms_length; i++)
            {
                if (m_array[i])
                {
                    yield return (SubType)i;
                }
            }
        }

        public override bool Equals(object obj)
        {
            SubTypes other = obj as SubTypes;
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return m_array.Equals(other.m_array);
        }

        public override int GetHashCode()
        {
            return m_array.GetHashCode();
        }

        public void ComputeHash(Hash hash, ObjectHash context)
        {
            m_array.ComputeHash(hash);
        }

        public static bool operator ==(SubTypes a, SubTypes b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(SubTypes a, SubTypes b)
        {
            return !(a == b);
        }

        public static SubTypes operator |(SubTypes a, SubType subType)
        {
            ContiguousBitArray clone = a.m_array.Clone();
            clone[(int)subType] = true;
            return new SubTypes(clone);
        }

        #endregion

        #region Inner Types

        [Serializable]
        private class ContiguousBitArray : IEquatable<ContiguousBitArray>
        {
            #region Constants

            private const int BitsPerWord = 64;

            #endregion

            #region Variables

            private readonly long[] m_words;

            #endregion

            #region Constructor

            public ContiguousBitArray(int length)
            {
                int numWords = (length + BitsPerWord - 1) / BitsPerWord;
                m_words = new long[numWords];
            }

            private ContiguousBitArray(ContiguousBitArray other)
            {
                m_words = (long[])other.m_words.Clone();
            }

            #endregion

            #region Properties

            public bool this[int index]
            {
                get
                {
                    return (m_words[WhichWord(index)] & MaskBit(index)) != 0;
                }
                set
                {
                    int wordIndex = WhichWord(index);

                    if (value)
                    {
                        m_words[wordIndex] |= MaskBit(index);
                    }
                    else
                    {
                        m_words[wordIndex] &= ~MaskBit(index);
                    }
                }
            }

            #endregion

            #region Methods

            public ContiguousBitArray Clone()
            {
                return new ContiguousBitArray(this);
            }

            public override bool Equals(object obj)
            {
                ContiguousBitArray array = obj as ContiguousBitArray;
                if (ReferenceEquals(array, null))
                {
                    return false;
                }

                return Equals(array);
            }

            public bool Equals(ContiguousBitArray other)
            {
                if (m_words.Length != other.m_words.Length)
                {
                    return false;
                }

                bool equals = true;

                for (int i = 0; i < m_words.Length; i++)
                {
                    equals &= (m_words[i] == other.m_words[i]);
                }

                return equals;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    // Naively return first word.
                    return (int)m_words[0];
                }
            }

            public void ComputeHash(Hash hash)
            {
                for (int i = 0; i < m_words.Length; i++)
                    hash.Add(m_words[i]);
            }

            private static int WhichWord(int pos)
            {
                return pos / BitsPerWord;
            }

            private static int WhichBit(int pos)
            {
                return pos % BitsPerWord;
            }

            private static long MaskBit(int pos)
            {
                return 1L << WhichBit(pos);
            }

            #endregion
        }

        #endregion
    }
}
