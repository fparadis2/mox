using System;

namespace Mox.UI.Browser
{
    public class DeckListPartViewModel : Child
    {
        #region Variables

        private readonly DeckLibraryViewModel m_deckLibrary;

        #endregion

        #region Constructor

        public DeckListPartViewModel(DeckLibraryViewModel deckLibrary)
        {
            m_deckLibrary = deckLibrary;
        }

        #endregion

        #region Properties

        public DeckLibraryViewModel DeckLibrary
        {
            get { return m_deckLibrary; }
        }

        #endregion

        #region Methods

        public void Edit(DeckViewModel deckViewModel)
        {
            var editor = deckViewModel.Editor.Clone();
            editor.IsEnabled = true;

            var editableDeck = new DeckViewModel(deckViewModel.Deck, editor);

            EditDeckPageViewModel viewModel = new EditDeckPageViewModel(m_deckLibrary, editableDeck);
            var parent = this.FindParent<INavigationConductor<INavigationViewModel<MoxWorkspace>>>();
            var pageHandle = parent.Push(viewModel);

            pageHandle.Closed += (o2, e) => Refresh();
        }

        #endregion
    }
}
