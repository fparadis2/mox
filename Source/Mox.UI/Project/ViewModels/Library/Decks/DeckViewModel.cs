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
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Mox.Database;

namespace Mox.UI.Library
{
    public class DeckViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly IDeck m_deck;

        private List<DeckCardViewModel> m_cards;
        private List<DeckCardGroupViewModel> m_groups;

        private readonly CompositeCollection m_groupsCompositeCollection = new CompositeCollection();

        #endregion

        #region Constructor

        public DeckViewModel(IDeck deck)
        {
            Throw.IfNull(deck, "deck");
            m_deck = deck;
        }

        #endregion

        #region Properties

        public IEnumerable<DeckCardViewModel> Cards
        {
            get
            {
                EnsureCardsCreated();
                return m_cards;
            }
        }

        public CompositeCollection CardGroupsCompositeCollection
        {
            get
            {
                EnsureCardsCreated();
                return m_groupsCompositeCollection;
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

        public string NumCardsText
        {
            get { return string.Format("{0} Cards", m_deck.Cards.Count); }
        }

        public string HeaderInfo
        {
            get
            {
                var description = m_deck.Description;
                if (string.IsNullOrEmpty(description))
                    return LastModificationTimeToolTipString;

                int index = description.IndexOfAny(new [] { '\r', '\n' });
                if (index >= 0)
                    description = description.Substring(0, index);

                return description;
            }
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
                NotifyOfPropertyChange(() => LastModificationTimeString);
                NotifyOfPropertyChange(() => LastModificationTimeToolTipString);
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

        private DeckCardViewModel m_hoveredCard;
        public DeckCardViewModel HoveredCard
        {
            get { return m_hoveredCard; }
            set
            {
                if (m_hoveredCard != value)
                {
                    m_hoveredCard = value;
                    NotifyOfPropertyChange();
                }
            }
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

                m_groups = new List<DeckCardGroupViewModel>();
                UpdateCardGrouping();

                if (m_groups.Count > 0)
                {
                    HoveredCard = m_groups[0].Cards.FirstOrDefault();
                }
            }
        }

        private void UpdateCardGrouping()
        {
            m_groups.Clear();

            switch (CardGrouping)
            {
                case DeckCardGrouping.Overview:
                    DeckCardGroupViewModel.GroupByType(m_groups, m_cards);
                    break;

                case DeckCardGrouping.Color:
                    DeckCardGroupViewModel.GroupByColor(m_groups, m_cards);
                    break;

                case DeckCardGrouping.Cost:
                    DeckCardGroupViewModel.GroupByCost(m_groups, m_cards);
                    break;

                case DeckCardGrouping.Rarity:
                    DeckCardGroupViewModel.GroupByRarity(m_groups, m_cards);
                    break;

                default:
                    throw new NotImplementedException();
            }

            m_groupsCompositeCollection.Clear();
            m_groupsCompositeCollection.Add(new CollectionContainer { Collection = m_groups });
            m_groupsCompositeCollection.Add(new DeckNumCardsViewModel { CardCount = m_deck.Cards.Count });

            //NotifyOfPropertyChange(() => CardGroupsCompositeCollection);

            // PropertyChanged for CardGroups is not sufficient to refresh the bindings for some reason
            //CollectionViewSource.GetDefaultView(m_groups).Refresh();
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

    public class DeckNumCardsViewModel
    {
        public int CardCount { get; set; }

        public string Text
        {
            get { return string.Format("{0} Cards", CardCount); }
        }
    }

    public class DeckViewModel_DesignTime : DeckViewModel
    {
        public DeckViewModel_DesignTime()
            : base(CreateDeck())
        { }

        private static IDeck CreateDeck()
        {
            return Deck.Read("Grinch Pocket Fringe UB", @"
// 10th edition starter deck
17 Plains
1 Ghost Warden
2 Youthful Knight
2 Benalish Knight
1 Venerable Monk
2 Wild Griffin
1 Cho-Manno, Revolutionary
2 Skyhunter Patrol
2 Angel of Mercy
2 Loxodon Mystic
1 Ancestor's Chosen
1 Condemn
2 Pacifism
1 Pariah
1 Serra's Embrace
1 Angel's Feather
1 Icy Manipulator
");
        }
    }
}