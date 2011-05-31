using System;

namespace Mox.UI.Browser
{
    public class InfoPanelPartViewModel
    {
        #region Variables

        private readonly DeckLibraryViewModel m_libraryViewModel;

        #endregion

        #region Constructor

        public InfoPanelPartViewModel(DeckLibraryViewModel libraryViewModel)
        {
            m_libraryViewModel = libraryViewModel;
        }

        #endregion

        #region Properties

        public DeckLibraryViewModel Library
        {
            get { return m_libraryViewModel; }
        }

        #endregion
    }
}
