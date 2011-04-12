using System;

namespace Mox.UI.Browser
{
    public class CardListPartViewModel
    {
        #region Variables

        private readonly CardCollectionViewModel m_cards;

        #endregion

        #region Constructor

        public CardListPartViewModel()
            : this(ViewModelDataSource.Instance.CardLibrary)
        {
        }

        protected CardListPartViewModel(CardCollectionViewModel cards)
        {
            m_cards = cards;
        }

        #endregion

        #region Properties

        public CardCollectionViewModel Cards
        {
            get { return m_cards; }
        }

        #endregion
    }
}
