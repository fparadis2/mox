using System;

namespace Mox.UI.Browser
{
    public class DeckListPartViewModel : Child
    {
        #region Variables

        private readonly CardCollectionViewModel m_cards;

        #endregion

        #region Constructor

        public DeckListPartViewModel()
            : this(CardLibraryViewModel.Instance)
        {
        }

        protected DeckListPartViewModel(CardCollectionViewModel cards)
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
