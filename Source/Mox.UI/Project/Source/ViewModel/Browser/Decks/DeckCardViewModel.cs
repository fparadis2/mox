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
using System.Windows.Input;

namespace Mox.UI.Browser
{
    public class DeckCardViewModel : CardViewModel
    {
        #region Variables

        private readonly DeckViewModel m_owner;
        private readonly CardIdentifier m_identifier;

        #endregion

        #region Constructor

        public DeckCardViewModel(DeckViewModel owner, CardIdentifier cardIdentifier)
            : base(owner.Editor.Database.Cards[cardIdentifier.Card], owner.Editor.CardFactory)
        {
            m_owner = owner;
            m_identifier = cardIdentifier;
        }

        #endregion

        #region Properties

        public bool IsEnabled
        {
            get { return m_owner.Editor.IsEnabled; }
        }

        public int Quantity
        {
            get { return m_owner.Deck.Cards[m_identifier]; }
            set
            {
                if (Quantity != value)
                {
                    value = Math.Max(0, value);
                    m_owner.Modify(deck => deck.Cards[m_identifier] = value);

                    m_owner.Refresh(this);
                    if (value <= 0)
                    {
                        m_owner.Remove(this);
                    }
                    OnPropertyChanged("Quantity");
                }
            }
        }

        public DeckCardGroupViewModel Group
        {
            get { return m_owner.GetOrCreateGroup(Card); }
        }

        #endregion

        #region Methods

        public void Increment()
        {
            Quantity += 1;
        }

        public void Decrement()
        {
            Quantity -= 1;
        }

        #region Commands

        public ICommand IncrementCommand
        {
            get { return new RelayCommand(o => true, o => Increment()); }
        }

        public ICommand DecrementCommand
        {
            get { return new RelayCommand(o => true, o => Decrement()); }
        }

        #endregion

        #endregion
    }
}
