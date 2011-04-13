using System;
using System.Windows;

namespace Mox.UI.Browser
{
    public class EditDeckCommandPartViewModel : Child
    {
        #region Variables

        private readonly DeckViewModel m_deck;
        private readonly DeckLibraryViewModel m_library;

        #endregion

        #region Constructor

        public EditDeckCommandPartViewModel(DeckLibraryViewModel library, DeckViewModel deck)
        {
            Throw.IfNull(library, "library");
            Throw.IfNull(deck, "deck");

            m_library = library;
            m_deck = deck;
        }

        #endregion

        #region Methods

        public void Cancel()
        {
            if (!m_deck.Editor.IsDirty || MessageService.ShowMessage("Are you sure you want to discard the changes made to this deck?", "Discard changes?", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                GoBack();
            }
        }

        public void Save()
        {
#warning TODO

            GoBack();
        }

        private void GoBack()
        {
            var shell = this.FindParent<INavigationConductor>();
            if (shell != null)
            {
                shell.Pop();
            }
        }

        #endregion
    }
}
