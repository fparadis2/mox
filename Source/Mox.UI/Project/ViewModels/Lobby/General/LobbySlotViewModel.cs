using System;
using Caliburn.Micro;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    public class LobbySlotViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly LobbyViewModel m_lobbyViewModel;
        private readonly int m_index;

        private LobbyUserViewModel m_user;

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

        public LobbyUserViewModel User
        {
            get { return m_user; }
            set
            {
                if (m_user != value)
                {
                    m_user = value;
                    NotifyOfPropertyChange(() => User);
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

        #endregion
    }
}
