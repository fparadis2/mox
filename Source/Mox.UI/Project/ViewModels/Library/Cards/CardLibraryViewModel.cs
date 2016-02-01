using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private readonly ICollectionView m_cardsView;

        private CardViewModel m_selectedCard;

        private string m_filterText;

        #endregion

        #region Constructor

        public CardLibraryViewModel(IEnumerable<CardInfo> cards)
        {
            m_cards = new List<CardViewModel>(cards.Select(card => new CardViewModel(card)));

            m_cards.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));

            m_cardsView = CollectionViewSource.GetDefaultView(m_cards);
            m_cardsView.Filter = FilterCard;
        }

        #endregion

        #region Properties

        public ICollectionView Cards
        {
            get { return m_cardsView; }
        }

        public CardViewModel SelectedCard
        {
            get { return m_selectedCard; }
            set
            {
                if (m_selectedCard != value)
                {
                    m_selectedCard = value;
                    NotifyOfPropertyChange();
                }
            }
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
            m_cardsView.Refresh();
        }

        private bool FilterCard(object o)
        {
            if (string.IsNullOrEmpty(FilterText))
                return true;

            CardViewModel cardModel = (CardViewModel)o;
            return cardModel.Name.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        #endregion
    }

    internal class CardLibraryViewModel_DesignTime : CardLibraryViewModel
    {
        public CardLibraryViewModel_DesignTime()
            : base(DesignTimeCardDatabase.Instance.Cards)
        {
        }
    }
}
