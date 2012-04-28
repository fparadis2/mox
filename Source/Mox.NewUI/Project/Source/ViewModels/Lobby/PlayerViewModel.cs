using System;
using System.Diagnostics;
using Caliburn.Micro;
using Mox.Database;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    public class PlayerViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly ILobby m_lobby;
        private readonly Guid m_identifier;
        private readonly DeckListViewModel m_list = new DeckListViewModel();

        private PlayerData m_data;
        private UserViewModel m_user;

        #endregion

        #region Constructor

        public PlayerViewModel(Mox.Lobby.Player player, UserViewModel user)
        {
            Throw.IfNull(player, "player");
            Throw.IfNull(user, "user");
            Throw.InvalidArgumentIf(player.User.Id != user.Id, "Inconsistent player/user pair", "user");

            m_identifier = player.Id;
            SyncFromPlayer(player, user);
        }

        public PlayerViewModel(Mox.Lobby.Player player, UserViewModel user, ILobby lobby)
            : this(player, user)
        {
            m_lobby = lobby;
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

        public DeckChoiceViewModel SelectedDeck
        {
            get { return m_data.Deck == null ? DeckChoiceViewModel.Random : new DeckChoiceViewModel(m_data.Deck); }
            set
            {
                if (!Equals(SelectedDeck, value))
                {
                    if (m_lobby != null)
                    {
                        var newData = m_data;
                        newData.Deck = value.Deck;

                        m_lobby.SetPlayerData(m_identifier, newData);
                    }
                    else
                    {
                        m_data.Deck = value.Deck;
                        NotifyOfPropertyChange(() => SelectedDeck);
                    }
                }
            }
        }

        #endregion

        #region Methods

        internal void SyncFromPlayer(Mox.Lobby.Player player, UserViewModel userViewModel)
        {
            Debug.Assert(player.Id == m_identifier);
            m_data = player.Data;
            User = userViewModel;

            NotifyOfPropertyChange(() => User);
            NotifyOfPropertyChange(() => SelectedDeck);
        }

        #endregion
    }
}