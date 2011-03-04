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

        private readonly ChatServiceBackend m_chatBackend;

        private LobbyState m_state = LobbyState.Open;

        #endregion

        #region Constructor

        public LobbyBackend(LobbyServiceBackend owner)
        {
            Throw.IfNull(owner, "owner");
            m_owner = owner;

            m_chatBackend = new ChatServiceBackend(Log);
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

        public ChatServiceBackend ChatService
        {
            get { return m_chatBackend; }
        }

        public ILog Log
        {
            get { return m_owner.Log; }
        }

        #endregion

        #region Methods

        internal bool Login(IClient client)
        {
            using (m_lock.Write)
            {
                if (m_state == LobbyState.Open)
                {
                    UserData userData = new UserData();
                    if (!m_users.ContainsKey(client.User))
                    {
                        m_users.Add(client.User, userData);
                        OnUserLogin(client);
                        return true;
                    }
                }
            }

            return false;
        }

        private void OnUserLogin(IClient client)
        {
#warning TODO: Use correct chat level
            m_chatBackend.Register(client.User, client.ChatClient, ChatLevel.Normal);
        }

        public void Logout(IClient client)
        {
            bool needToClose = false;

            using (m_lock.Write)
            {
                if (m_users.Remove(client.User))
                {
                    OnUserLogout(client);
                }

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

        private void OnUserLogout(IClient client)
        {
            m_chatBackend.Unregister(client.User);
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
