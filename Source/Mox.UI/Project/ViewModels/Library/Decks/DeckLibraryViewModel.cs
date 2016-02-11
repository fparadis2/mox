using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Mox.Database;

namespace Mox.UI.Library
{
    public class DeckLibraryViewModel : Screen
    {
        #region Variables

        private readonly DeckLibrary m_library;

        private readonly List<DeckViewModel> m_decks;
        private readonly ICollectionView m_decksView;

        private DeckViewModel m_selectedDeck;

        private string m_filterText;

        #endregion

        #region Constructor

        public DeckLibraryViewModel(DeckLibrary library)
        {
            Throw.IfNull(library, "library");

            m_library = library;
            m_decks = new List<DeckViewModel>(library.Decks.Select(d => new DeckViewModel(d)));
            m_decksView = CollectionViewSource.GetDefaultView(m_decks);
            m_decksView.Filter = FilterDeck;

            m_selectedDeck = m_decks.FirstOrDefault();

            DisplayName = "Decks";
        }

        protected override void OnDeactivate(bool close)
        {
              base.OnDeactivate(close);
        }

        #endregion

        #region Properties

        public ICollectionView Decks
        {
            get { return m_decksView; }
        }

        public DeckViewModel SelectedDeck
        {
            get { return m_selectedDeck; }
            set
            {
                if (m_selectedDeck != value)
                {
                    m_selectedDeck = value;
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
            m_decksView.Refresh();
        }

        private bool FilterDeck(object o)
        {
            if (string.IsNullOrEmpty(FilterText))
                return true;

            DeckViewModel deckModel = (DeckViewModel)o;
            return deckModel.Name.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        #endregion
    }
}
