using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Mox.Collections;

namespace Mox.UI.Lobby
{
    public class DeckListViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly ObservableCollection<DeckViewModel> m_decks = new ObservableCollection<DeckViewModel>();

        #endregion

        #region Properties

        public ICollection<DeckViewModel> Decks
        {
            get { return m_decks; }
        }

        #endregion
    }
}
