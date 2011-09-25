using System;
using System.Diagnostics;
using Caliburn.Micro;

namespace Mox.UI.Lobby
{
    public class PlayerViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly Guid m_identifier;
        private UserViewModel m_user;

        #endregion

        #region Constructor

        public PlayerViewModel(Mox.Lobby.Player player, UserViewModel user)
        {
            Throw.IfNull(player, "player");
            Throw.IfNull(user, "user");

            m_identifier = player.Id;
            User = user;
        }

        #endregion

        #region Properties

        public Guid Id
        {
            get { return m_identifier; }
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

        #endregion

        #region Methods

        internal void SyncFromPlayer(Mox.Lobby.Player player, UserViewModel userViewModel)
        {
            Debug.Assert(player.Id == m_identifier);
            User = userViewModel;
        }

        #endregion
    }
}