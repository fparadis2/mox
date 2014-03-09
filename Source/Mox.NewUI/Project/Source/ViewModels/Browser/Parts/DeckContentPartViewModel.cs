using System;

namespace Mox.UI.Browser
{
    public class DeckContentPartViewModel
    {
        #region Variables

        private readonly DeckLibraryViewModel m_libraryViewModel;

        #endregion

        #region Constructor

        public DeckContentPartViewModel(DeckLibraryViewModel libraryViewModel)
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

    internal class DeckContentPartViewModel_DesignTime : DeckContentPartViewModel
    {
        public DeckContentPartViewModel_DesignTime()
            : base(new DeckLibraryViewModel_DesignTime())
        {
        }
    }
}
