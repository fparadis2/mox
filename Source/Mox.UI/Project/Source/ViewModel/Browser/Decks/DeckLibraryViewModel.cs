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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Mox.Database;

namespace Mox.UI.Browser
{
    public class DeckLibraryViewModel : ViewModel
    {
        #region Variables

        private readonly IDeckViewModelEditor m_editor;
        private readonly DeckLibrary m_library;
        private readonly ObservableCollection<DeckViewModel> m_decks;
        private readonly CollectionViewSource m_collectionViewSource = new CollectionViewSource();

        private string m_filter;
        private DeckViewModel m_selectedDeck;

        #endregion

        #region Constructor

        public DeckLibraryViewModel(IDeckViewModelEditor editor, DeckLibrary library)
        {
            m_editor = editor;
            m_library = library;
            m_decks = new ObservableCollection<DeckViewModel>(m_library.Decks.Select(CreateViewModel));
            m_collectionViewSource.Source = m_decks;
        }

        #endregion

        #region Properties

        protected IList<DeckViewModel> Decks
        {
            get { return m_decks; }
        }

        public CollectionViewSource DecksViewSource
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

        public DeckViewModel SelectedDeck
        {
            get { return m_selectedDeck; }
            set
            {
                if (m_selectedDeck != value)
                {
                    m_selectedDeck = value;
                    OnPropertyChanged("SelectedDeck");
                }
            }
        }

        #endregion

        #region Methods

        public DeckViewModel Add(Deck deck)
        {
            Throw.IfNull(deck, "deck");

            PrepareDeck(deck);

            m_library.Save(deck);

            DeckViewModel newDeckModel = CreateViewModel(deck);
            m_decks.Add(newDeckModel);

            SelectedDeck = newDeckModel;

            return newDeckModel;
        }

        private void PrepareDeck(Deck deck)
        {
            if (string.IsNullOrEmpty(deck.Author))
            {
                deck.Author = m_editor.UserName;
            }
        }

        private DeckViewModel CreateViewModel(Deck deck)
        {
            return new DeckViewModel(m_editor, deck);
        }

        private void RefreshFilter()
        {
            m_collectionViewSource.View.Filter = o =>
            {
                DeckViewModel cardModel = (DeckViewModel)o;
                return cardModel.Name.Contains(Filter);
            };
            m_collectionViewSource.View.Refresh();
        }

        #endregion
    }
}
