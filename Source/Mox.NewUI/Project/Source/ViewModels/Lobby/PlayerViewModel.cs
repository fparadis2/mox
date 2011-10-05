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
        private readonly DeckChoiceViewModel m_deckChoice = new DeckChoiceViewModel();

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

        public DeckChoiceViewModel DeckChoice
        {
            get { return m_deckChoice; }
        }

        #endregion

        #region Methods

        internal void SyncFromPlayer(Mox.Lobby.Player player, UserViewModel userViewModel)
        {
            Debug.Assert(player.Id == m_identifier);
            m_data = player.Data;
            User = userViewModel;

            DeckChoice.UseRandomDeck = m_data.UseRandomDeck;
            DeckChoice.SelectedDeck = m_data.Deck == null ? null : new DeckViewModel(m_data.Deck);
        }

        #endregion
    }
}