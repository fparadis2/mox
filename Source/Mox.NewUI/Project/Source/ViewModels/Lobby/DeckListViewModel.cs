using System;
using System.Collections.Generic;
using System.Windows.Data;
using Caliburn.Micro;
using Mox.Database;

namespace Mox.UI.Lobby
{
    public class DeckListViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly List<DeckChoiceViewModel> m_decks = new List<DeckChoiceViewModel>();
        private readonly CollectionView m_decksView;

        #endregion

        #region Constructor

        public DeckListViewModel()
        {
            m_decks.Add(DeckChoiceViewModel.Random);

            foreach (var deck in ViewModelDataSource.Instance.DeckLibrary.Decks)
            {
                m_decks.Add(new DeckChoiceViewModel(deck));
            }

            m_decksView = new CollectionView(m_decks);
        }

        #endregion

        #region Properties

        public ICollection<DeckChoiceViewModel> DecksSource
        {
            get { return m_decks; }
        }

        public CollectionView Decks
        {
            get { return m_decksView; }
        }

        #endregion

        #region Methods

        public Deck GetDeck(Guid deckId)
        {
            return ViewModelDataSource.Instance.DeckLibrary.GetDeck(deckId);
        }

        #endregion
    }
}
