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
            EditDeckPageViewModel viewModel = new EditDeckPageViewModel(m_deckLibrary, deckViewModel);
            var parent = this.FindParent<INavigationConductor<INavigationViewModel<MoxWorkspace>>>();
            var pageHandle = parent.Push(viewModel);

            pageHandle.Closed += (o2, e) => Refresh();
        }

        public DeckViewModel CreateDeck()
        {
            var newDeckViewModel = m_deckLibrary.Add(new Database.Deck());
            Edit(newDeckViewModel);
            return newDeckViewModel;
        }

        #endregion
    }
}
