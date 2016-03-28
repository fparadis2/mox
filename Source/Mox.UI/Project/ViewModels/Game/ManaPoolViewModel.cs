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
using System.Windows.Input;
using Caliburn.Micro;

namespace Mox.UI.Game
{
    public class ManaPoolViewModel : PropertyChangedBase
    {
        #region Inner Types

        public class ManaPoolElementViewModel : PropertyChangedBase
        {
            #region Variables

            private readonly ManaPoolViewModel m_manaPool;
            private readonly Color m_color;

            private int m_amount;
            private bool m_canPay;

            #endregion

            #region Constructor

            public ManaPoolElementViewModel(ManaPoolViewModel manaPool, Color color)
            {
                m_manaPool = manaPool;
                m_color = color;
            }

            #endregion

            #region Properties

            public Color Color
            {
                get { return m_color; }
            }

            public int Amount
            {
                get { return m_amount; }
                set
                {
                    if (m_amount != value)
                    {
                        m_amount = value;
                        NotifyOfPropertyChange();
                        NotifyOfPropertyChange(() => Value);
                        NotifyOfPropertyChange(() => IsEmpty);
                    }
                }
            }

            public bool CanPay
            {
                get { return m_canPay; }
                set
                {
                    if (m_canPay != value)
                    {
                        m_canPay = value;
                        NotifyOfPropertyChange();
                    }
                }
            }

            public string Value
            {
                get
                {
                    switch (m_color)
                    {
                        case Color.None:
                            return "{C} " + Amount;
                        case Color.Black:
                            return "{B} " + Amount;
                        case Color.Blue:
                            return "{U} " + Amount;
                        case Color.White:
                            return "{W} " + Amount;
                        case Color.Green:
                            return "{G} " + Amount;
                        case Color.Red:
                            return "{R} " + Amount;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            public bool IsEmpty
            {
                get { return Amount == 0; }
            }

            public ICommand PayCommand
            {
                get { return new RelayCommand(() => m_manaPool.PayMana(this), () => CanPay); }
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly ManaPoolElementViewModel m_colorless;
        private readonly ManaPoolElementViewModel m_black;
        private readonly ManaPoolElementViewModel m_blue;
        private readonly ManaPoolElementViewModel m_white;
        private readonly ManaPoolElementViewModel m_green;
        private readonly ManaPoolElementViewModel m_red;

        #endregion

        #region Constructor

        public ManaPoolViewModel()
        {
            m_colorless = new ManaPoolElementViewModel(this, Color.None);
            m_black = new ManaPoolElementViewModel(this, Color.Black);
            m_blue = new ManaPoolElementViewModel(this, Color.Blue);
            m_white = new ManaPoolElementViewModel(this, Color.White);
            m_green = new ManaPoolElementViewModel(this, Color.Green);
            m_red = new ManaPoolElementViewModel(this, Color.Red);
        }

        #endregion

        #region Properties

        public IEnumerable<ManaPoolElementViewModel> AllMana
        {
            get
            {
                yield return m_white;
                yield return m_blue;
                yield return m_black;
                yield return m_red;
                yield return m_green;
                yield return m_colorless;
            }
        }

        public ManaPoolElementViewModel Colorless
        {
            get { return m_colorless; }
        }

        public ManaPoolElementViewModel Black
        {
            get { return m_black; }
        }

        public ManaPoolElementViewModel Blue
        {
            get { return m_blue; }
        }

        public ManaPoolElementViewModel White
        {
            get { return m_white; }
        }

        public ManaPoolElementViewModel Green
        {
            get { return m_green; }
        }

        public ManaPoolElementViewModel Red
        {
            get { return m_red; }
        }

        public ManaPoolElementViewModel this[Color color]
        {
            get
            {
                switch (color)
                {
                    case Color.None:
                        return m_colorless;
                    case Color.Black:
                        return m_black;
                    case Color.Blue:
                        return m_blue;
                    case Color.White:
                        return m_white;
                    case Color.Green:
                        return m_green;
                    case Color.Red:
                        return m_red;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        #endregion

        #region Methods

        internal void ResetInteraction()
        {
            foreach (var mana in AllMana)
            {
                mana.CanPay = false;
            }
        }

        public void PayMana(ManaPoolElementViewModel element)
        {
            if (element.CanPay)
            {
                OnManaPaid(new ItemEventArgs<Color>(element.Color));
            }
        }

        /// <summary>
        /// Triggered when player pays some mana
        /// </summary>
        public event EventHandler<ItemEventArgs<Color>> ManaPaid;

        /// <summary>
        /// Triggers the ManaPaid event.
        /// </summary>
        protected void OnManaPaid(ItemEventArgs<Color> e)
        {
            ManaPaid.Raise(this, e);
        }

        #endregion
    }

    public class ManaPoolViewModel_DesignTime : ManaPoolViewModel
    {
        public ManaPoolViewModel_DesignTime()
        {
            Red.Amount = 10;
            Colorless.Amount = 2;
            White.Amount = 3;
            Blue.Amount = 3;
            Blue.CanPay = true;
        }
    }
}
