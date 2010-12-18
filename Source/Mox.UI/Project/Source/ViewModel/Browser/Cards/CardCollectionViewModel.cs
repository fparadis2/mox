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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using Mox.Database;

namespace Mox.UI.Browser
{
    public class CardCollectionViewModel : ViewModel
    {
        #region Variables

        private readonly List<CardViewModel> m_cards;
        private readonly CollectionViewSource m_collectionViewSource = new CollectionViewSource();

        private string m_filter;

        #endregion

        #region Constructor

        public CardCollectionViewModel(IEnumerable<CardInfo> cards, IMasterCardFactory cardFactory)
        {
            m_cards = cards.Select(card => new CardViewModel(card, cardFactory)).ToList();
            m_collectionViewSource.Source = m_cards;
        }

        #endregion

        #region Properties

        protected IList<CardViewModel> Cards
        {
            get { return m_cards; }
        }

        public CollectionViewSource CardsViewSource
        {
            get { return m_collectionViewSource; }
        }

        public string Filter
        {
            get { return m_filter; }
            set
            {
                if (m_filter != value)
                {
                    m_filter = value;

                    RefreshFilter();

                    OnPropertyChanged("Filter");
                }
            }
        }

        #endregion

        #region Methods

        public static CardCollectionViewModel FromMaster()
        {
            return new CardCollectionViewModel(MasterCardDatabase.Instance.Cards, MasterCardFactory.Instance);
        }

        private void RefreshFilter()
        {
            m_collectionViewSource.View.Filter = o =>
            {
                CardViewModel cardModel = (CardViewModel)o;
                return cardModel.Name.Contains(Filter);
            };
            m_collectionViewSource.View.Refresh();
        }

        #endregion
    }
}
