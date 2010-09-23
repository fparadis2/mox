using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mox.Database;

namespace Mox.UI.Browser
{
    public class BrowseDecksPageViewModel : PageViewModel
    {
        #region Variables

        private readonly EditDeckViewModel m_editorModel;
        private readonly DeckLibraryViewModel m_libraryModel;

        #endregion

        #region Constructor

        public BrowseDecksPageViewModel(DeckLibrary library, CardDatabase cardDatabase)
        {
            m_editorModel = new EditDeckViewModel(cardDatabase);
            m_libraryModel = new DeckLibraryViewModel(m_editorModel, library);
        }

        #endregion

        #region Properties

        public IDeckViewModelEditor Editor
        {
            get { return m_editorModel; }
        }

        public DeckLibraryViewModel Library
        {
            get { return m_libraryModel; }
        }

        public override string Title
        {
            get { return "Deck library"; }
        }

        #endregion
    }
}
