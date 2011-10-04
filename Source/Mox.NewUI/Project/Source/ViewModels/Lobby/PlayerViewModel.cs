using System;
using System.Diagnostics;
using Caliburn.Micro;
using Mox.Lobby;
using Mox.UI.Browser;

namespace Mox.UI.Lobby
{
    public class PlayerViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly Guid m_identifier;

        private PlayerData m_data;

        private UserViewModel m_user;
        private DeckViewModel m_deck;

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

        #endregion

        #region Properties

        public Guid Id
        {
            get { return m_identifier; }
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

        public DeckViewModel Deck
        {
            get { return m_deck; }
            set
            {
                if (m_deck != value)
                {
                    m_deck = value;
                    NotifyOfPropertyChange(() => Deck);
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
            Deck = m_data.Deck == null ? null : new DeckViewModel(m_data.Deck, DeckViewModelEditor.FromMaster());
        }

        #endregion
    }
}