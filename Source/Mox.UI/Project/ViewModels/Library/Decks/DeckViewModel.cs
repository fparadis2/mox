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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Caliburn.Micro;
using Mox.Database;

namespace Mox.UI.Library
{
    public class DeckCardViewModel
    {
        public DeckCardViewModel(CardIdentifier card, int quantity)
        {
            Card = card;
            Quantity = quantity;
        }

        public CardIdentifier Card
        {
            get; 
            private set; 
        }

        public int Quantity
        {
            get;
            private set;
        }
    }

    public class DeckViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly IDeck m_deck;

        private List<DeckCardViewModel> m_cards;
        private ICollectionView m_cardsView;

        #endregion

        #region Constructor

        public DeckViewModel(IDeck deck)
        {
            Throw.IfNull(deck, "deck");
            m_deck = deck;
        }

        #endregion

        #region Properties

        public ICollectionView Cards
        {
            get
            {
                EnsureCardsCreated();
                return m_cardsView;
            }
        }

        public string Name
        {
            get { return m_deck.Name; }
        }

        public string Description
        {
            get { return m_deck.Description; }
        }

        private DeckCardGrouping m_cardGrouping;

        public DeckCardGrouping CardGrouping
        {
            get { return m_cardGrouping; }
            set
            {
                m_cardGrouping = value;
                NotifyOfPropertyChange();

                UpdateCardGrouping();
            }
        }

        private DateTime m_lastModificationTime;
        public DateTime LastModificationTime
        {
            get { return m_lastModificationTime; }
            set
            {
                m_lastModificationTime = value;
                NotifyOfPropertyChange();
            }
        }

        public string LastModificationTimeString
        {
            get { return new DateTimeOffset(DateTime.UtcNow, LastModificationTime).ToString(); }
        }

        public string LastModificationTimeToolTipString
        {
            get { return string.Format("Last modified {0}", LastModificationTimeString); }
        }

        #endregion

        #region Methods

        private void EnsureCardsCreated()
        {
            if (m_cards == null)
            {
                m_cards = new List<DeckCardViewModel>();

                foreach (var grouping in m_deck.Cards.GroupBy(c => c))
                {
                    m_cards.Add(new DeckCardViewModel(grouping.Key, grouping.Count()));
                }

                UpdateCardGrouping();

                m_cardsView = CollectionViewSource.GetDefaultView(m_cards);
                m_cardsView.GroupDescriptions.Add(new PropertyGroupDescription { PropertyName = "Group" });
                m_cardsView.SortDescriptions.Add(new SortDescription { PropertyName = "Group" });
                m_cardsView.SortDescriptions.Add(new SortDescription { PropertyName = "Name" });
            }
        }

        private void UpdateCardGrouping()
        {
            // TODO
        }

        public void InvalidateTimingBasedProperties()
        {
            NotifyOfPropertyChange(() => LastModificationTimeString);
            NotifyOfPropertyChange(() => LastModificationTimeToolTipString);
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }

    public enum DeckCardGrouping
    {
        Overview
    }
}