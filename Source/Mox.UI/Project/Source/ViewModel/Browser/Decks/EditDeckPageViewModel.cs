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
using System.Windows;
using Mox.Database;

namespace Mox.UI.Browser
{
    public class EditDeckPageViewModel : PageViewModel
    {
        #region Variables

        private readonly DeckLibrary m_library;
        private readonly EditDeckViewModel m_editorModel;
        private readonly DeckViewModel m_deckViewModel;
        private readonly CardCollectionViewModel m_cardLibraryViewModel;

        #endregion

        #region Constructor

        protected EditDeckPageViewModel(DeckLibraryViewModel libraryViewModel, CardDatabase cardDatabase, Deck deck, CardCollectionViewModel cardCollectionViewModel)
        {
            m_library = libraryViewModel.Library;
            m_editorModel = new EditDeckViewModel(cardDatabase)
            {
                IsEnabled = true
            };

            m_editorModel.PropertyChanged += m_editorModel_PropertyChanged;
            m_deckViewModel = new DeckViewModel(libraryViewModel, m_editorModel, deck);
            m_cardLibraryViewModel = cardCollectionViewModel;
        }

        public EditDeckPageViewModel(DeckLibraryViewModel library, CardDatabase cardDatabase, Deck deck)
            : this(library, cardDatabase, deck, new CardLibraryViewModel())
        {
        }

        public EditDeckPageViewModel(DeckLibraryViewModel library, IDeckViewModelEditor editor, Deck deck)
            : this(library, editor.Database, deck)
        {
        }

        #endregion

        #region Properties

        public IDeckViewModelEditor Editor
        {
            get { return m_editorModel; }
        }

        public CardCollectionViewModel CardLibraryViewModel
        {
            get { return m_cardLibraryViewModel; }
        }

        private DeckLibrary Library
        {
            get { return m_library; }
        }

        public DeckViewModel DeckViewModel
        {
            get { return m_deckViewModel; }
        }

        public override string Title
        {
            get { return "Edit Deck"; }
        }

        #endregion

        #region Navigation

        public override string GoBackText
        {
            get { return "Cancel"; }
        }

        public override void GoBack()
        {
            if (!Editor.IsDirty || MessageService.ShowMessage("Are you sure you want to discard the changes made to this deck?", "Discard changes?", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                base.GoBack();
            }
        }

        public override bool CanGoForward
        {
            get
            {
                return Editor.IsDirty;
            }
        }

        public override string GoForwardText
        {
            get { return "Save"; }
        }

        public override void GoForward()
        {
            Library.Save(m_deckViewModel.Deck);

            if (GameFlow.Instance.CanGoBack)
            {
                GameFlow.Instance.GoBack();
            }
        }

        #endregion

        #region Event Handlers

        void m_editorModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("CanGoForward");
        }

        #endregion
    }
}
