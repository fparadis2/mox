using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Mox.Database;

namespace Mox.UI.Library
{
    public class CardLibraryViewModel : Screen
    {
        #region Variables

        private readonly List<CardViewModel> m_cards;
        private readonly CollectionViewSource m_cardsViewSource = new CollectionViewSource();

        private string m_filterText;

        #endregion

        #region Constructor

        public CardLibraryViewModel(IEnumerable<CardInfo> cards)
        {
            m_cards = cards.Select(card => new CardViewModel(card)).ToList();
            m_cardsViewSource.Source = m_cards;
            m_cardsViewSource.View.Filter = FilterCard;
        }

        #endregion

        #region Properties

        protected IList<CardViewModel> Cards
        {
            get { return m_cards; }
        }

        public CollectionViewSource CardsViewSource
        {
            get { return m_cardsViewSource; }
        }

        public string FilterText
        {
            get { return m_filterText; }
            set
            {
                if (m_filterText != value)
                {
                    m_filterText = value;

                    RefreshFilter();
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion

        #region Methods

        private void RefreshFilter()
        {
            m_cardsViewSource.View.Refresh();
        }

        private bool FilterCard(object o)
        {
            if (string.IsNullOrEmpty(FilterText))
                return true;

            CardViewModel cardModel = (CardViewModel)o;
            return cardModel.Name.Contains(FilterText);
        }

        #endregion

    }
}
