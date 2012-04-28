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
using Caliburn.Micro;

namespace Mox.UI.Game
{
    public class ManaPoolViewModel : PropertyChangedBase
    {
        #region Inner Types

        public class ManaPoolMember<T> : PropertyChangedBase
        {
            #region Variables

            private readonly T[] m_mana = new T[6];

            #endregion

            #region Constructor

            internal ManaPoolMember()
            {
            }

            #endregion

            #region Properties

            public T Colorless
            {
                get { return this[Color.None]; }
                set { this[Color.None] = value; }
            }

            public T Black
            {
                get { return this[Color.Black]; }
                set { this[Color.Black] = value; }
            }

            public T Blue
            {
                get { return this[Color.Blue]; }
                set { this[Color.Blue] = value; }
            }

            public T White
            {
                get { return this[Color.White]; }
                set { this[Color.White] = value; }
            }

            public T Green
            {
                get { return this[Color.Green]; }
                set { this[Color.Green] = value; }
            }

            public T Red
            {
                get { return this[Color.Red]; }
                set { this[Color.Red] = value; }
            }

            public T this[Color color]
            {
                get { return m_mana[GetIndex(color)]; }
                set
                {
                    int index = GetIndex(color);
                    if (!Equals(m_mana[index], value))
                    {
                        m_mana[index] = value;
                        OnPropertyChanged(color);
                    }
                }
            }

            #endregion

            #region Methods

            private void OnPropertyChanged(Color color)
            {
                NotifyOfPropertyChange(color == Color.None ? "Colorless" : color.ToString());
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

            #endregion
        }

        #endregion

        #region Variables

        private readonly ManaPoolMember<int> m_mana = new ManaPoolMember<int>();
        private readonly ManaPoolMember<bool> m_canPay = new ManaPoolMember<bool>();

        #endregion

        #region Properties

        public ManaPoolMember<int> Mana
        {
            get { return m_mana; }
        }

        public ManaPoolMember<bool> CanPay
        {
            get { return m_canPay; }
        }

        #endregion

        #region Methods

        internal void ResetInteraction()
        {
            foreach (Color color in Enum.GetValues(typeof(Color)))
            {
                CanPay[color] = false;
            }
        }

        #endregion
    }
}
