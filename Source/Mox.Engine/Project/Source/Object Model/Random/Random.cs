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
using System.Linq;

namespace Mox
{
    /// <summary>
    /// Extension methods using <see cref="IRandom"/>.
    /// </summary>
    public static class Random
    {
        #region Methods

        /// <summary>
        /// Creates a new <see cref="IRandom"/> object.
        /// </summary>
        /// <returns></returns>
        public static IRandom New()
        {
            return new DefaultRandom();
        }

        /// <summary>
        /// Creates a new <see cref="IRandom"/> object using the given seed.
        /// </summary>
        /// <returns></returns>
        public static IRandom New(int seed)
        {
            return new DefaultRandom(seed);
        }

        /// <summary>
        /// Provides n repositioning factors that are gonna be used in a Fisher-Yates shuffling algorithm.
        /// </summary>
        /// <returns></returns>
        public static int[] Shuffle(this IRandom random, int n)
        {
            int[] indices = new int[n];

            while (n > 1)
            {
                int k = random.Next(n);
                n--;
                indices[n] = k;
            }

            return indices;
        }

        /// <summary>
        /// Returns an element from the given <paramref name="collection"/> at random.
        /// </summary>
        public static T Choose<T>(this IRandom random, IEnumerable<T> collection)
        {
            IList<T> genericList = collection as IList<T>;
            if (genericList != null)
                return genericList[random.Next(genericList.Count)];

            IList list = collection as IList;
            if (list != null)
                return (T)list[random.Next(list.Count)];

            throw new InvalidOperationException();
        }

        #endregion

        #region Inner Types

        private sealed class DefaultRandom : IRandom
        {
            #region Variables

            private readonly System.Random m_random;

            #endregion

            #region Constructor

            public DefaultRandom()
            {
                m_random = new System.Random();
            }

            public DefaultRandom(int seed)
            {
                m_random = new System.Random(seed);
            }

            #endregion

            #region Methods

            public int Next()
            {
                return m_random.Next();
            }

            public int Next(int max)
            {
                return m_random.Next(max);
            }

            public int Next(int min, int max)
            {
                return m_random.Next(min, max);
            }

            #endregion
        }

        #endregion
    }
}
