using System;

namespace Mox.UI.Browser
{
    public class EditDeckPageViewModel : MoxNavigationViewModel
    {
        #region Variables

        private readonly CardListPartViewModel m_cards;
        private readonly EditDeckCommandPartViewModel m_command;

        private readonly DeckLibraryViewModel m_deckLibrary;
        private readonly DeckViewModel m_deck;

        #endregion

        #region Constructor

        public EditDeckPageViewModel(DeckLibraryViewModel deckLibrary, DeckViewModel deck)
        {
            Throw.IfNull(deckLibrary, "deckLibrary");
            Throw.IfNull(deck, "deck");

            m_deckLibrary = deckLibrary;
            m_deck = deck;

            m_cards = ActivatePart(new CardListPartViewModel());
            m_command = ActivatePart(new EditDeckCommandPartViewModel(deckLibrary, deck));
        }

        #endregion

        #region Properties

        public DeckLibraryViewModel DeckLibrary
        {
            get { return m_deckLibrary; }
        }

        public DeckViewModel EditedDeck
        {
            get { return m_deck; }
        }

        #endregion

        #region Methods

        public override void Fill(MoxWorkspace view)
        {
            view.LeftView = m_cards;
            view.CommandView = m_command;
        }

        #endregion
    }
}
