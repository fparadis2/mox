﻿// Copyright (c) François Paradis
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
using System.Diagnostics;
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

        protected ManaAmount m_mana;

        #endregion

        #region Constructor

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
            m_mana = other.m_mana;
        }

        #endregion

        #region Properties

        public byte this[Color color]
        {
            get
            {
                switch (color)
                {
                    case Color.None: return m_mana.Colorless;
                    case Color.White: return m_mana.White;
                    case Color.Blue: return m_mana.Blue;
                    case Color.Black: return m_mana.Black;
                    case Color.Red: return m_mana.Red;
                    case Color.Green: return m_mana.Green;

                    default:
                        throw new NotSupportedException();
                }
            }
            set
            {
                switch (color)
                {
                    case Color.None: Colorless = value; break;
                    case Color.White: White = value; break;
                    case Color.Blue: Blue = value; break;
                    case Color.Black: Black = value; break;
                    case Color.Red: Red = value; break;
                    case Color.Green: Green = value; break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public byte Colorless
        {
            get { return m_mana.Colorless; }
            set
            {
                var newValue = m_mana;
                if (newValue.Colorless != value)
                {
                    newValue.Colorless = value;
                    SetMana(newValue);
                }
            }
        }

        public byte White
        {
            get { return m_mana.White; }
            set
            {
                var newValue = m_mana;
                if (newValue.White != value)
                {
                    newValue.White = value;
                    SetMana(newValue);
                }
            }
        }

        public byte Blue
        {
            get { return m_mana.Blue; }
            set
            {
                var newValue = m_mana;
                if (newValue.Blue != value)
                {
                    newValue.Blue = value;
                    SetMana(newValue);
                }
            }
        }

        public byte Black
        {
            get { return m_mana.Black; }
            set
            {
                var newValue = m_mana;
                if (newValue.Black != value)
                {
                    newValue.Black = value;
                    SetMana(newValue);
                }
            }
        }

        public byte Red
        {
            get { return m_mana.Red; }
            set
            {
                var newValue = m_mana;
                if (newValue.Red != value)
                {
                    newValue.Red = value;
                    SetMana(newValue);
                }
            }
        }

        public byte Green
        {
            get { return m_mana.Green; }
            set
            {
                var newValue = m_mana;
                if (newValue.Green != value)
                {
                    newValue.Green = value;
                    SetMana(newValue);
                }
            }
        }

        #endregion

        #region Methods

        public void Clear()
        {
            SetMana(new ManaAmount());
        }

        protected virtual void SetMana(ManaAmount value)
        {
            m_mana = value;
        }

        public bool CanPay(ManaPaymentAmount amount)
        {
            return
                m_mana.Colorless >= amount.Colorless &&
                m_mana.White >= amount.White &&
                m_mana.Blue >= amount.Blue &&
                m_mana.Black >= amount.Black &&
                m_mana.Red >= amount.Red &&
                m_mana.Green >= amount.Green;
        }

        public void Pay(ManaPaymentAmount amount)
        {
            Debug.Assert(m_mana.Colorless >= amount.Colorless);
            Debug.Assert(m_mana.White >= amount.White);
            Debug.Assert(m_mana.Blue >= amount.Blue);
            Debug.Assert(m_mana.Black >= amount.Black);
            Debug.Assert(m_mana.Red >= amount.Red);
            Debug.Assert(m_mana.Green >= amount.Green);

            var newMana = m_mana;

            newMana.Colorless -= amount.Colorless;
            newMana.White -= amount.White;
            newMana.Blue -= amount.Blue;
            newMana.Black -= amount.Black;
            newMana.Red -= amount.Red;
            newMana.Green -= amount.Green;

            SetMana(newMana);
        }

        public static implicit operator ManaAmount(ManaPool pool)
        {
            return pool.m_mana;
        }

        #endregion
    }
}
