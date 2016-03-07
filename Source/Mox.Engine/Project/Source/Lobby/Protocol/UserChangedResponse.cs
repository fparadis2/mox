using System;
using System.Collections.Generic;

namespace Mox.Lobby.Network.Protocol
{
    public enum UserChange
    {
        Joined,
        Left
    }

    [Serializable]
    public class UserChangedResponse : Message
    {
        #region Variables

        private readonly UserChange m_change;
        private readonly List<User> m_users;

        #endregion

        #region Constructor

        public UserChangedResponse(UserChange change, IEnumerable<User> users)
        {
            m_change = change;
            m_users = new List<User>(users);
        }

        #endregion

        #region Properties

        public IList<User> Users
        {
            get { return m_users; }
        }

        public UserChange Change
        {
            get { return m_change; }
        }

        #endregion
    }
}
