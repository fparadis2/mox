using System;
using System.Windows.Input;
using Mox.Database;

namespace Mox.UI.Library
{
    public class DeckBrowsePageViewModel : PageViewModel
    {
        #region Variables

        private readonly DeckLibraryViewModel m_library;

        #endregion

        #region Constructor

        public DeckBrowsePageViewModel(DeckLibrary library)
        {
            DisplayName = "Select a Deck";
            m_library = new DeckLibraryViewModel(library);
            m_library.DeckAccepted += WhenDeckAccepted;
        }

        private void WhenDeckAccepted(object sender, EventArgs e)
        {
            if (CanAccept())
                Accept();
        }

        #endregion

        #region Properties

        public DeckLibraryViewModel Library
        {
            get { return m_library; }
        }

        public DeckViewModel SelectedDeck
        {
            get { return m_library.SelectedDeck; }
        }

        #endregion

        #region Commands

        public ICommand AcceptCommand
        {
            get { return new RelayCommand(Accept, CanAccept); }
        }

        public Func<DeckBrowsePageViewModel, bool> AcceptAction { get; set; }

        public bool CanAccept()
        {
            return SelectedDeck != null;
        }

        public void Accept()
        {
            if (AcceptAction != null && !AcceptAction(this))
            {
                return;
            }

            Close();
        }

        #endregion
    }
}
