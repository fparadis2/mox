using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mox.Lobby.Backend
{
    public partial class LobbyBackend
    {
        #region Variables

        private static readonly MessageRouter<LobbyBackend> ms_router = new MessageRouter<LobbyBackend>();

        private readonly LobbyServiceBackend m_owner;
        private readonly ChatServiceBackend m_chat;
        private readonly GameBackend m_game;

        private readonly Guid m_id = Guid.NewGuid();
        private readonly object m_lock = new object();

        private readonly UserCollection m_users = new UserCollection();
        private readonly PlayerCollection m_players;

        private LobbyState m_state = LobbyState.Open;

        #endregion

        #region Constructor

        static LobbyBackend()
        {
            ms_router.Register<ChatMessage>(lobby => lobby.Say);
            ms_router.Register<SetPlayerDataRequest, SetPlayerDataResponse>(lobby => lobby.SetPlayerData);
            ms_router.Register<StartGameRequest>(lobby => lobby.StartGame);
        }

        public LobbyBackend(LobbyServiceBackend owner)
        {
            Throw.IfNull(owner, "owner");
            m_owner = owner;

            m_chat = new ChatServiceBackend(owner.Log);
            m_game = new GameBackend(owner.Log);
            m_players = new PlayerCollection(this);

            m_players.Initialize();
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
                lock (m_lock)
                {
                    return m_users.AllUsers;
                }
            }
        }

        public IList<Player> Players
        {
            get { return m_players.Players; }
        }

        public ILog Log
        {
            get { return m_owner.Log; }
        }

        #endregion

        #region Methods

        #region Connection

        private void WhenMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            ms_router.Route(this, (IChannel)sender, e.Message);
        }

        #endregion

        #region User Management

        internal bool Login(IChannel channel, User user)
        {
            UserInternalData userData = new UserInternalData(user);

            lock (m_lock)
            {
                if (m_state != LobbyState.Open || m_users.Contains(channel))
                {
                    return false;
                }

                channel.MessageReceived += WhenMessageReceived;

                m_chat.Register(user, channel, ChatLevel.Normal);

#warning [MEDIUM] Only assign player for non-spectators
                m_players.AssignUser(user);

                m_users.Add(channel, userData);
                SendUserJoinMessages(channel, user);
            }

            return true;
        }

        public void Logout(IChannel channel, string reason)
        {
            bool needToClose = false;

            lock (m_lock)
            {
                UserInternalData userData;
                if (m_users.Remove(channel, out userData))
                {
                    Log.Log(LogImportance.Normal, "{0} left lobby {1} ({2})", userData.User, Id, reason);

                    channel.MessageReceived -= WhenMessageReceived;

                    m_chat.Unregister(channel);
                    m_players.UnassignUser(userData.User);

                    SendUserLeaveMessages(userData, reason);
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

        public bool TryGetChannel(User user, out IChannel channel)
        {
            return m_users.TryGetChannel(user, out channel);
        }

        #endregion

        #region User Data Management

        public SetPlayerDataResult SetPlayerData(IChannel channel, Guid playerId, PlayerData playerData)
        {
            lock (m_lock)
            {
                UserInternalData userData;
                int playerIndex;
                if (!m_users.TryGetUser(channel, out userData) || !m_players.TryGetPlayer(playerId, out playerIndex))
                {
                    return SetPlayerDataResult.InvalidPlayer;
                }

                if (!CanSetPlayerData(userData, Players[playerIndex]))
                {
                    return SetPlayerDataResult.UnauthorizedAccess;
                }

                m_players.ChangeData(playerIndex, playerData);
            }

            return SetPlayerDataResult.Success;
        }

        private SetPlayerDataResponse SetPlayerData(IChannel channel, SetPlayerDataRequest request)
        {
            var result = SetPlayerData(channel, request.PlayerId, request.PlayerData);
            return new SetPlayerDataResponse { Result = result };
        }

        private static bool CanSetPlayerData(UserInternalData userData, Player player)
        {
            if (userData.User == player.User)
            {
                return true; // Can change our own player
            }

            return false;
        }

        #endregion

        #region Chat

        private void Say(IChannel channel, ChatMessage message)
        {
            m_chat.Say(channel, message.Message);
        }

        #endregion

        #region Game

        private void StartGame(IChannel channel, StartGameRequest message)
        {
#warning [Medium] TODO: Check that the starter is the lobby leader
#warning [Medium] TODO: Check that everyone is ready
            m_game.StartGame(this);
        }

        #endregion

        #endregion

        #region Inner Types

        private class UserInternalData
        {
            #region Variables

            private readonly User m_user;

            #endregion

            #region Constructor

            public UserInternalData(User user)
            {
                Throw.IfNull(user, "user");
                m_user = user;
            }

            #endregion

            #region Properties

            public User User
            {
                get { return m_user; }
            }

            #endregion
        }

        private enum LobbyState
        {
            Open,
            Closed
        }

        private class UserCollection
        {
            #region Variables

            private readonly Dictionary<IChannel, UserInternalData> m_users = new Dictionary<IChannel, UserInternalData>();
            private readonly Dictionary<User, IChannel> m_channels = new Dictionary<User, IChannel>();

            #endregion

            #region Properties

            public User[] AllUsers
            {
                get { return m_users.Values.Select(d => d.User).ToArray(); }
            }

            public IChannel[] Channels
            {
                get { return m_users.Keys.ToArray(); }
            }

            public int Count
            {
                get { return m_users.Count; }
            }

            #endregion

            #region Methods

            public bool Contains(IChannel channel)
            {
                return m_users.ContainsKey(channel);
            }

            public bool TryGetUser(IChannel channel, out UserInternalData userData)
            {
                return m_users.TryGetValue(channel, out userData);
            }

            public bool TryGetChannel(User user, out IChannel channel)
            {
                return m_channels.TryGetValue(user, out channel);
            }

            public void Add(IChannel channel, UserInternalData data)
            {
                m_users.Add(channel, data);
                m_channels.Add(data.User, channel);
            }

            public bool Remove(IChannel channel, out UserInternalData data)
            {
                if (m_users.TryGetValue(channel, out data))
                {
                    bool removed = m_users.Remove(channel);
                    Debug.Assert(removed);

                    removed = m_channels.Remove(data.User);
                    Debug.Assert(removed);

                    return true;
                }

                return false;
            }

            #endregion
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

            public void Initialize()
            {
                const int NumPlayers = 2;

                while (NumPlayers > m_internalCollection.Count)
                {
                    Player player = new Player(User.CreateAIUser());
                    m_internalCollection.Add(player);
                }

                if (NumPlayers < m_internalCollection.Count)
                {
                    // TODO: Kick corresponding users?
                    // TODO: Trigger PlayerChange.Left events
                    m_internalCollection.RemoveRange(NumPlayers, m_internalCollection.Count - NumPlayers);
                }

                Debug.Assert(m_internalCollection.Count == NumPlayers);
            }

            public bool TryGetPlayer(Guid playerId, out int playerIndex)
            {
                return TryFindSlot(p => p.Id == playerId, out playerIndex);
            }

            public void ChangeData(int index, PlayerData data)
            {
                var newPlayer = m_internalCollection[index].ChangeData(data);
                m_internalCollection[index] = newPlayer;
                m_backend.SendPlayerChangedMessages(newPlayer);
            }

            public void AssignUser(User user)
            {
                int index;
                if (TryFindFreeSlot(out index))
                {
                    var newPlayer = m_internalCollection[index].AssignUser(user);
                    m_internalCollection[index] = newPlayer;
                    m_backend.SendPlayerChangedMessages(newPlayer);
                }
            }

            public void UnassignUser(User user)
            {
                int index;
                if (TryFindUserSlot(user, out index))
                {
                    var newAiPlayer = m_internalCollection[index].AssignUser(User.CreateAIUser());
                    m_internalCollection[index] = newAiPlayer;
                    m_backend.SendPlayerChangedMessages(newAiPlayer);
                }
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
