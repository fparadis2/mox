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
using System.Linq;
using System.Text;

namespace Mox
{
    /// <summary>
    /// A mana pool.
    /// </summary>
    public class ManaPool
    {
        #region Variables

        private readonly int[] m_mana = new int[6];

        #endregion

        #region Methods

        /// <summary>
        /// Constructs an empty pool.
        /// </summary>
        public ManaPool()
        {
        }

        /// <summary>
        /// Constructs a pool from an existing one (returns an independent copy).
        /// </summary>
        /// <param name="other"></param>
        public ManaPool(ManaPool other)
        {
            m_mana = (int[])other.m_mana.Clone();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the amount of mana of the given <paramref name="color"/> in the mana pool.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public int this[Color color]
        {
            get
            {
                return m_mana[GetIndex(color)];
            }
            set
            {
                Throw.ArgumentOutOfRangeIf(value < 0, "Mana value cannot be negative", "value");
                if (this[color] != value)
                {
                    SetMana(color, value);
                }
            }
        }

        public int Colorless
        {
            get { return this[Color.None]; }
            set { this[Color.None] = value; }
        }

        public int White
        {
            get { return this[Color.White]; }
            set { this[Color.White] = value; }
        }

        public int Blue
        {
            get { return this[Color.Blue]; }
            set { this[Color.Blue] = value; }
        }

        public int Black
        {
            get { return this[Color.Black]; }
            set { this[Color.Black] = value; }
        }

        public int Red
        {
            get { return this[Color.Red]; }
            set { this[Color.Red] = value; }
        }

        public int Green
        {
            get { return this[Color.Green]; }
            set { this[Color.Green] = value; }
        }

        /// <summary>
        /// Total mana count in the mana pool (of all colors).
        /// </summary>
        public int TotalManaAmount
        {
            get
            {
                return m_mana.Sum();
            }
        }

        public bool TryGetSingleColor(ref Color color)
        {
            bool singleColor = false;

            for (int i = 0; i < m_mana.Length; i++)
            {
                if (m_mana[i] > 0)
                {
                    if (singleColor)
                    {
                        singleColor = false;
                        break;
                    }

                    singleColor = true;
                    color = GetColor(i);
                }
            }

            return singleColor;
        }

        #endregion

        #region Methods

        protected virtual void SetMana(Color color, int amount)
        {
            m_mana[GetIndex(color)] = amount;
        }

        private static int GetIndex(Color color)
        {
            switch (color)
            {
                case Color.None:
                    return 0;
                case Color.White:
                    return 1;
                case Color.Blue:
                    return 2;
                case Color.Black:
                    return 3;
                case Color.Red:
                    return 4;
                case Color.Green:
                    return 5;
                default:
                    throw new NotSupportedException("Invalid color");
            }
        }

        private static Color GetColor(int index)
        {
            switch (index)
            {
                case 0:
                    return Color.None;
                case 1:
                    return Color.White;
                case 2:
                    return Color.Blue;
                case 3:
                    return Color.Black;
                case 4:
                    return Color.Red;
                case 5:
                    return Color.Green;
                default:
                    throw new NotSupportedException("Invalid index");
            }
        }

        #endregion
    }
}
