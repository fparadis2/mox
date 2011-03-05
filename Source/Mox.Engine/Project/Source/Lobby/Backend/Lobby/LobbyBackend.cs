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

        private IEnumerable<UserData> UserDatas
        {
            get
            {
                m_lock.AssertCanRead();
                return m_users.Values.ToArray();
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
            IEnumerable<UserData> existingUsers;
            UserData userData = new UserData(client);

            using (m_lock.Write)
            {
                if (m_state != LobbyState.Open || m_users.ContainsKey(client.User))
                {
                    return false;
                }
                
                OnUserLogin(client);
                existingUsers = UserDatas;
                m_users.Add(client.User, userData);
            }

            SendUserJoin(existingUsers, userData);
            return true;
        }

        private void OnUserLogin(IClient client)
        {
#warning TODO: Use correct chat level
            m_chatBackend.Register(client.User, client.ChatClient, ChatLevel.Normal);
        }

        public void Logout(IClient client)
        {
            bool needToClose = false;
            IEnumerable<UserData> existingUsers = Enumerable.Empty<UserData>();

            using (m_lock.Write)
            {
                if (m_users.Remove(client.User))
                {
                    existingUsers = UserDatas;
                    OnUserLogout(client);
                }

                if (m_users.Count == 0)
                {
                    m_state = LobbyState.Closed;
                    needToClose = true;
                }
            }

            SendUserLeft(existingUsers, client.User);

            if (needToClose)
            {
                m_owner.DestroyLobby(this);
            }
        }

        private void OnUserLogout(IClient client)
        {
            m_chatBackend.Unregister(client.User);
        }

        private static void SendUserJoin(IEnumerable<UserData> oldUsers, UserData newUser)
        {
            foreach (var user in oldUsers)
            {
                user.Client.OnUserChanged(UserChange.Joined, newUser.User);
            }

            foreach (var user in oldUsers)
            {
                newUser.Client.OnUserChanged(UserChange.Joined, user.User);
            }
        }

        private static void SendUserLeft(IEnumerable<UserData> otherUsers, User leavingUser)
        {
            foreach (var user in otherUsers)
            {
                user.Client.OnUserChanged(UserChange.Left, leavingUser);
            }
        }

        #endregion

        #region Inner Types

        private class UserData
        {
            #region Variables

            private readonly IClient m_client;

            #endregion

            #region Constructor

            public UserData(IClient client)
            {
                Throw.IfNull(client, "client");
                m_client = client;
            }

            #endregion

            #region Properties

            public User User
            {
                get { return m_client.User; }
            }

            public IClient Client
            {
                get { return m_client; }
            }

            #endregion
        }

        private enum LobbyState
        {
            Open,
            Closed
        }

        #endregion
    }
}
