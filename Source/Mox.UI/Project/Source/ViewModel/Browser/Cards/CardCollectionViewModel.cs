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

        public CardCollectionViewModel(IEnumerable<CardInfo> cards)
        {
            m_cards = cards.Select(card => new CardViewModel(card)).ToList();
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
