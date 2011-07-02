using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

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
            m_deck.BeginEdit();
        }

        #endregion

        #region Properties

        public ICommand SaveCommand
        {
            get { return new RelayCommand(o => CanSave(), o => Save()); }
        }

        #endregion

        #region Methods

        public void Cancel()
        {
            if (!m_deck.IsDirty || MessageService.ShowMessage("Are you sure you want to discard the changes made to this deck?", "Discard changes?", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                m_deck.CancelEdit();
                GoBack();
            }
        }

        public bool CanSave()
        {
            IDataErrorInfo dataErrorInfo = m_deck;
            return string.IsNullOrEmpty(dataErrorInfo.Error);
        }

        public void Save()
        {
            m_deck.EndEdit();
            m_library.Library.Save(m_deck.Deck);

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
