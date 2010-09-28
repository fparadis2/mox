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

        private readonly EditDeckViewModel m_editorModel;
        private readonly DeckLibrary m_library;
        private readonly DeckViewModel m_deckViewModel;

        private bool m_isDirty;

        #endregion

        #region Constructor

        public EditDeckPageViewModel(DeckLibrary library, CardDatabase cardDatabase, Deck deck)
        {
            m_editorModel = new EditDeckViewModel(cardDatabase);
            m_library = library;
            m_deckViewModel = new DeckViewModel(m_editorModel, deck);
        }

        #endregion

        #region Properties

        public IDeckViewModelEditor Editor
        {
            get { return m_editorModel; }
        }

        public override string Title
        {
            get { return "Edit Deck"; }
        }

        public bool IsDirty
        {
            get { return m_isDirty; }
            set 
            {
                if (m_isDirty != value)
                {
                    m_isDirty = value;
                    OnPropertyChanged("IsDirty");
                }
            }
        }

        #endregion

        #region Navigation

        public override string GoBackText
        {
            get { return "Cancel"; }
        }

        public override void GoBack()
        {
            if (!IsDirty || MessageService.ShowMessage("Are you sure you want to discard the changes made to this deck?", "Discard changes?", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                base.GoBack();
            }
        }

        public override bool CanGoForward
        {
            get
            {
                return IsDirty;
            }
        }

        public override string GoForwardText
        {
            get { return "Save"; }
        }

        public override void GoForward()
        {
#warning TODO: Validate deck properties (empty name?)

            m_library.Save(m_deckViewModel.Deck);

            if (GameFlow.Instance.CanGoBack)
            {
                GameFlow.Instance.GoBack();
            }
        }

        #endregion
    }
}
