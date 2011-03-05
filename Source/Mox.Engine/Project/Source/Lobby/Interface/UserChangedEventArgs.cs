using System;

namespace Mox.Lobby
{
    public enum UserChange
    {
        Joined,
        Left
    }

    public class UserChangedEventArgs : EventArgs
    {
        #region Variables

        private readonly UserChange m_change;
        private readonly User m_user;

        #endregion

        #region Constructor

        public UserChangedEventArgs(UserChange change, User user)
        {
            m_change = change;
            m_user = user;
        }

        #endregion

        #region Properties

        public User User
        {
            get { return m_user; }
        }

        public UserChange Change
        {
            get { return m_change; }
        }

        #endregion
    }
}
