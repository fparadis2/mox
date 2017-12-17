using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Mox.Database;

namespace Mox.UI.Library
{
    public class CardLibraryViewModel : Screen
    {
        #region Variables

        private readonly CollectionView<CardViewModel> m_cards = new CollectionView<CardViewModel>();

        private string m_filterText;

        #endregion

        #region Constructor

        public CardLibraryViewModel(IEnumerable<CardInfo> cards)
        {
            m_cards.PageSize = 50;
            m_cards.SortComparer = (a, b) => string.CompareOrdinal(a.Name, b.Name);
            m_cards.Filter = FilterCard;
            m_cards.Reset(cards.Select(card => new CardViewModel(card)));
        }

        #endregion

        #region Properties

        public IReadOnlyCollection<CardViewModel> Cards
        {
            get { return m_cards.Items; }
        }

        public string FilterText
        {
            get { return m_filterText; }
            set
            {
                if (m_filterText != value)
                {
                    m_filterText = value;

                    m_cards.Refresh();
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion

        #region Methods

        private bool FilterCard(CardViewModel card)
        {
            if (string.IsNullOrEmpty(FilterText))
                return true;

            return card.Name.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        #endregion
    }

    internal class CardLibraryViewModel_DesignTime : CardLibraryViewModel
    {
        public CardLibraryViewModel_DesignTime()
            : base(DesignTimeCardDatabase.Instance.Cards)
        {
            DisplayName = "My Library";
        }
    }
}
