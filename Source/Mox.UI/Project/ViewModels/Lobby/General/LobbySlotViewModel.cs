using System;
using System.Windows.Input;
using Caliburn.Micro;
using Mox.Database;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    public class LobbySlotViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly LobbyViewModel m_lobbyViewModel;
        private readonly int m_index;

        #endregion

        #region Constructor

        public LobbySlotViewModel(LobbyViewModel lobbyViewModel, int index)
        {
            Throw.IfNull(lobbyViewModel, "lobbyViewModel");
            m_lobbyViewModel = lobbyViewModel;
            m_index = index;
        }

        #endregion

        #region Properties

        public string SlotName
        {
            get { return string.Format("Slot {0}", m_index + 1); }
        }

        public bool CanChangeSlot
        {
            get { return User == null || m_lobbyViewModel.LocalUser == User; }
        }

        public string UserName
        {
            get { return User == null ? "Unassigned" : User.Name; }
        }

        private LobbyUserViewModel m_user;
        public LobbyUserViewModel User
        {
            get { return m_user; }
            set
            {
                if (m_user != value)
                {
                    m_user = value;
                    NotifyOfPropertyChange();
                    NotifyOfPropertyChange(() => CanChangeSlot);
                    NotifyOfPropertyChange(() => UserName);
                }
            }
        }

        private DeckChoiceViewModel m_deck = DeckChoiceViewModel.Random;
        public DeckChoiceViewModel Deck
        {
            get { return m_deck; }
            set
            {
                Throw.IfNull(value, "Deck");

                if (m_deck != value)
                {
                    m_deck = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion

        #region Methods

        public void SyncFromModel(PlayerSlot slot)
        {
            LobbyUserViewModel user;
            m_lobbyViewModel.TryGetUserViewModel(slot.User, out user);
            User = user;
        }

        public ICommand BrowseDeckCommand
        {
            get { return new RelayCommand(BrowseDeck); }
        }

        private void BrowseDeck()
        {
            
        }

        public ICommand SetRandomDeckCommand
        {
            get { return new RelayCommand(SetRandomDeck); }
        }

        private void SetRandomDeck()
        { }

        #endregion

        #region Nested

        public class DeckChoiceViewModel
        {
            public static readonly DeckChoiceViewModel Random = new DeckChoiceViewModel(null);

            private readonly IDeck m_deck;

            public DeckChoiceViewModel(IDeck deck)
            {
                m_deck = deck;
            }

            public string Name
            {
                get
                {
                    if (m_deck != null)
                        return m_deck.Name;

                    return "Random Deck";
                }
            }
        }

        #endregion
    }

    public class LobbySlotViewModel_DesignTime : LobbySlotViewModel
    {
        public LobbySlotViewModel_DesignTime() 
            : base(new LobbyViewModel(), 0)
        {
        }
    }
}
