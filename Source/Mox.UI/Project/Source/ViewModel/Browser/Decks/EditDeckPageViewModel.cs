using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            base.GoBack();
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

            if (GameFlow.CanGoBack)
            {
                GameFlow.GoBack();
            }
        }

        #endregion
    }
}
