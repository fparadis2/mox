using System;
using System.Diagnostics;
using Caliburn.Micro;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    public class PlayerViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly Guid m_identifier;
        private readonly DeckListViewModel m_list;

        private PlayerData m_data;
        private UserViewModel m_user;

        #endregion

        #region Constructor

        public PlayerViewModel(DeckListViewModel list, Mox.Lobby.Player player, UserViewModel user)
        {
            Throw.IfNull(list, "list");
            Throw.IfNull(player, "player");
            Throw.IfNull(user, "user");
            Throw.InvalidArgumentIf(player.User.Id != user.Id, "Inconsistent player/user pair", "user");

            m_identifier = player.Id;
            m_list = list;
            SyncFromPlayer(player, user);
        }

        public PlayerViewModel(DeckListViewModel deckList, Mox.Lobby.Player player, UserViewModel user, ILobby lobby)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Properties

        public Guid Id
        {
            get { return m_identifier; }
        }

        public DeckListViewModel DeckList
        {
            get { return m_list; }
        }

        public PlayerData Data
        {
            get { return m_data; }
        }

        public UserViewModel User
        {
            get { return m_user; }
            set
            {
                Throw.IfNull(value, "User");

                if (m_user != value)
                {
                    m_user = value;
                    NotifyOfPropertyChange(() => User);
                }
            }
        }

        public DeckViewModel SelectedDeck
        {
#warning todo
            get { return null; }
            //get { return m_data.Deck == null ? null : new DeckViewModel(m_data.Deck); }
            set
            {
                if (SelectedDeck != value)
                {
                    //m_data.Deck = value.Deck;
                    NotifyOfPropertyChange(() => SelectedDeck);
                    NotifyOfPropertyChange(() => SelectedDeckName);
                }
            }
        }

        public bool UseRandomDeck
        {
#warning todo remove
            get { return false; }
            set
            {
                if (UseRandomDeck != value)
                {
                    NotifyOfPropertyChange(() => UseRandomDeck);
                    NotifyOfPropertyChange(() => SelectedDeckName);
                }
            }
        }

        public string SelectedDeckName
        {
            get
            {
                if (UseRandomDeck)
                {
                    return "Random Deck";
                }

                if (SelectedDeck != null)
                {
                    return SelectedDeck.Name;
                }

                return "[No selected deck]";
            }
        }

        #endregion

        #region Methods

        internal void SyncFromPlayer(Mox.Lobby.Player player, UserViewModel userViewModel)
        {
            Debug.Assert(player.Id == m_identifier);
            m_data = player.Data;
            User = userViewModel;
        }

        #endregion
    }
}