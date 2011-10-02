using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mox.Lobby.Backend
{
    public class LobbyBackend
    {
        #region Variables

        private readonly LobbyServiceBackend m_owner;
        private readonly Guid m_id = Guid.NewGuid();
        private readonly ReadWriteLock m_lock = ReadWriteLock.CreateNoRecursion();

        private readonly Dictionary<User, UserInternalData> m_users = new Dictionary<User, UserInternalData>();
        private readonly PlayerCollection m_players;

        private readonly ChatServiceBackend m_chatBackend;

        private LobbyState m_state = LobbyState.Open;
        private GameInfo m_gameInfo = new GameInfo();

        #endregion

        #region Constructor

        public LobbyBackend(LobbyServiceBackend owner)
        {
            Throw.IfNull(owner, "owner");
            m_owner = owner;

            m_chatBackend = new ChatServiceBackend(Log);
            m_players = new PlayerCollection(this);

            m_players.Initialize(m_gameInfo);
        }

        #endregion

        #region Properties

        public Guid Id
        {
            get { return m_id; }
        }

        public IList<User> Users
        {
            get
            {
                using (m_lock.Read)
                {
                    return m_users.Keys.ToArray();
                }
            }
        }

        public IList<Player> Players
        {
            get { return m_players.Players; }
        }

        private IEnumerable<UserInternalData> UserDatas
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

        public GameInfo GameInfo
        {
            get { return m_gameInfo.Clone(); }
        }

        #endregion

        #region Methods

        #region User Management

        internal bool Login(IClient client)
        {
            IEnumerable<UserInternalData> existingUsers;
            IEnumerable<Player> existingPlayers;
            UserInternalData userData = new UserInternalData(client);
            Player newPlayer;

            using (m_lock.Write)
            {
                if (m_state != LobbyState.Open || m_users.ContainsKey(client.User))
                {
                    return false;
                }

#warning TODO: Use correct chat level
                m_chatBackend.Register(client.User, client.ChatClient, ChatLevel.Normal);

#warning TODO: Only assign player for non-spectators
                newPlayer = m_players.AssignUser(client.User);

                existingUsers = UserDatas;
                existingPlayers = m_players.Players.ToList();
                m_users.Add(client.User, userData);
            }

            SendEventsWhenUserJoins(existingUsers, existingPlayers, userData, newPlayer);
            return true;
        }

        public void Logout(IClient client)
        {
            bool needToClose = false;
            IEnumerable<UserInternalData> existingUsers = Enumerable.Empty<UserInternalData>();
            Player playerQuitting = null;

            using (m_lock.Write)
            {
                if (m_users.Remove(client.User))
                {
                    existingUsers = UserDatas;
                    m_chatBackend.Unregister(client.User);
                    playerQuitting = m_players.UnassignUser(client.User);
                }

                if (m_users.Count == 0)
                {
                    m_state = LobbyState.Closed;
                    needToClose = true;
                }
            }

            SendEventsWhenUserLeaves(existingUsers, client.User, playerQuitting);

            if (needToClose)
            {
                m_owner.DestroyLobby(this);
            }
        }

        private static void SendEventsWhenUserJoins(IEnumerable<UserInternalData> oldUsers, IEnumerable<Player> players, UserInternalData newUser, Player newPlayer)
        {
            foreach (var user in oldUsers)
            {
                user.Client.OnUserChanged(UserChange.Joined, newUser.User);
            }

            foreach (var user in oldUsers)
            {
                newUser.Client.OnUserChanged(UserChange.Joined, user.User);
            }

            foreach (var player in players)
            {
                newUser.Client.OnPlayerChanged(PlayerChange.Joined, player);
            }

            if (newPlayer != null)
            {
                foreach (var user in oldUsers)
                {
                    user.Client.OnPlayerChanged(PlayerChange.Changed, newPlayer);
                }
            }
        }

        private static void SendEventsWhenUserLeaves(IEnumerable<UserInternalData> otherUsers, User leavingUser, Player replacingPlayer)
        {
            foreach (var user in otherUsers)
            {
                user.Client.OnUserChanged(UserChange.Left, leavingUser);
            }

            if (replacingPlayer != null)
            {
                foreach (var user in otherUsers)
                {
                    user.Client.OnPlayerChanged(PlayerChange.Changed, replacingPlayer);
                }
            }
        }

        #endregion

        #region User Data Management

        public SetPlayerDataResult SetPlayerData(IClient client, Guid playerId, PlayerData data)
        {
            IEnumerable<UserInternalData> existingUsers = Enumerable.Empty<UserInternalData>();
            Player newPlayer;

            using (m_lock.Write)
            {
                UserInternalData userData;
                int playerIndex;
                if (!m_users.TryGetValue(client.User, out userData) || !m_players.TryGetPlayer(playerId, out playerIndex))
                {
                    return SetPlayerDataResult.InvalidPlayer;
                }

                if (!CanSetPlayerData(userData, Players[playerIndex]))
                {
                    return SetPlayerDataResult.UnauthorizedAccess;
                }

                existingUsers = UserDatas;
                newPlayer = m_players.ChangeData(playerIndex, data);
            }

            foreach (var user in existingUsers)
            {
                user.Client.OnPlayerChanged(PlayerChange.Changed, newPlayer);
            }

            return SetPlayerDataResult.Success;
        }

        private static bool CanSetPlayerData(UserInternalData user, Player player)
        {
            if (user.User == player.User)
            {
                return true; // Can change our own player
            }

            return false;
        }

        #endregion

        #endregion

        #region Inner Types

        private class UserInternalData
        {
            #region Variables

            private readonly IClient m_client;

            #endregion

            #region Constructor

            public UserInternalData(IClient client)
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

        private class PlayerCollection
        {
            #region Variables

            private readonly LobbyBackend m_backend;
            private readonly List<Player> m_internalCollection = new List<Player>();

            #endregion

            #region Constructor

            public PlayerCollection(LobbyBackend backend)
            {
                Throw.IfNull(backend, "backend");
                m_backend = backend;
            }

            #endregion

            #region Properties

            public IList<Player> Players
            {
                get { return m_internalCollection.AsReadOnly(); }
            }

            #endregion

            #region Methods

            public void Initialize(GameInfo gameInfo)
            {
                while (gameInfo.NumberOfPlayers > m_internalCollection.Count)
                {
                    Player player = new Player(User.CreateAIUser());
                    m_internalCollection.Add(player);
                }

                if (gameInfo.NumberOfPlayers < m_internalCollection.Count)
                {
                    // TODO: Kick corresponding users?
                    // TODO: Trigger PlayerChange.Left events
                    m_internalCollection.RemoveRange(gameInfo.NumberOfPlayers, m_internalCollection.Count - gameInfo.NumberOfPlayers);
                }

                Debug.Assert(m_internalCollection.Count == gameInfo.NumberOfPlayers);
            }

            public bool TryGetPlayer(Guid playerId, out int playerIndex)
            {
                return TryFindSlot(p => p.Id == playerId, out playerIndex);
            }

            public Player ChangeData(int index, PlayerData data)
            {
                var newPlayer = m_internalCollection[index].ChangeData(data);
                m_internalCollection[index] = newPlayer;
                return newPlayer;
            }

            public Player AssignUser(User user)
            {
                int index;
                if (TryFindFreeSlot(out index))
                {
                    var newPlayer = m_internalCollection[index].AssignUser(user);
                    m_internalCollection[index] = newPlayer;
                    return newPlayer;
                }
                return null;
            }

            public Player UnassignUser(User user)
            {
                int index;
                if (TryFindUserSlot(user, out index))
                {
                    var newAiPlayer = m_internalCollection[index].AssignUser(User.CreateAIUser());
                    m_internalCollection[index] = newAiPlayer;
                    return newAiPlayer;
                }
                return null;
            }

            private bool TryFindUserSlot(User user, out int index)
            {
                return TryFindSlot(p => p.User == user, out index);
            }

            private bool TryFindFreeSlot(out int index)
            {
                return TryFindSlot(p => p.User.IsAI, out index);
            }

            private bool TryFindSlot(Func<Player, bool> predicate, out int index)
            {
                for (int i = 0; i < m_internalCollection.Count; i++)
                {
                    if (predicate(m_internalCollection[i]))
                    {
                        index = i;
                        return true;
                    }
                }

                index = -1;
                return false;
            }

            #endregion
        }

        #endregion
    }
}
