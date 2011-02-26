using System;
using System.Collections.Generic;
using System.Linq;

namespace Mox.Lobby.Backend
{
    public class LobbyBackend
    {
        #region Variables

        private readonly LobbyServiceBackend m_owner;
        private readonly Guid m_id = Guid.NewGuid();
        private readonly ReadWriteLock m_lock = ReadWriteLock.CreateNoRecursion();

        private readonly Dictionary<User, UserData> m_users = new Dictionary<User, UserData>();

        private LobbyState m_state = LobbyState.Open;

        #endregion

        #region Constructor

        public LobbyBackend(LobbyServiceBackend owner)
        {
            Throw.IfNull(owner, "owner");
            m_owner = owner;
        }

        #endregion

        #region Properties

        public Guid Id
        {
            get { return m_id; }
        }

        public IEnumerable<User> Users
        {
            get
            {
                using (m_lock.Read)
                {
                    return m_users.Keys.ToArray();
                }
            }
        }

        #endregion

        #region Methods

        public bool Login(User user)
        {
            using (m_lock.Write)
            {
                if (m_state == LobbyState.Open)
                {
                    UserData userData = new UserData();
                    if (!m_users.ContainsKey(user))
                    {
                        m_users.Add(user, userData);
                        return true;
                    }
                }
            }

            return false;
        }

        public void Logout(User user)
        {
            bool needToClose = false;

            using (m_lock.Write)
            {
                m_users.Remove(user);

                if (m_users.Count == 0)
                {
                    m_state = LobbyState.Closed;
                    needToClose = true;
                }
            }

            if (needToClose)
            {
                m_owner.DestroyLobby(this);
            }
        }

        #endregion

        #region Inner Types

        private class UserData
        {
        }

        private enum LobbyState
        {
            Open,
            Closed
        }

        #endregion
    }
}
