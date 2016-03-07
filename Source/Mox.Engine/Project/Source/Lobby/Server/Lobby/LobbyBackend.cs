using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mox.Lobby.Network;
using Mox.Lobby.Network.Protocol;

namespace Mox.Lobby.Server
{
    public partial class LobbyBackend
    {
        #region Variables

        private static readonly MessageRouter<LobbyBackend> ms_router = new MessageRouter<LobbyBackend>();

        private readonly LobbyParameters m_lobbyParameters;
        private readonly LobbyServiceBackend m_owner;
        private readonly ChatServiceBackend m_chat;
        private readonly GameBackend m_game;

        private readonly Guid m_id = Guid.NewGuid();
        private readonly object m_lock = new object();

        private readonly UserCollection m_users = new UserCollection();
        private readonly List<PlayerSlot> m_slots = new List<PlayerSlot>();

        private LobbyState m_state = LobbyState.Open;

        #endregion

        #region Constructor

        static LobbyBackend()
        {
            ms_router.Register<ChatMessage>(lobby => lobby.Say);
            ms_router.Register<GetLobbyDetailsRequest, GetLobbyDetailsResponse>(lobby => lobby.GetLobbyDetails);
            ms_router.Register<SetPlayerSlotDataRequest, SetPlayerSlotDataResponse>(lobby => lobby.SetPlayerSlotData);
            ms_router.Register<StartGameRequest>(lobby => lobby.StartGame);
        }

        public LobbyBackend(LobbyServiceBackend owner, LobbyParameters lobbyParameters)
        {
            Throw.IfNull(owner, "owner");
            Throw.IfNull(lobbyParameters.GameFormat, "GameFormat");
            Throw.IfNull(lobbyParameters.DeckFormat, "DeckFormat");
            m_owner = owner;
            m_lobbyParameters = lobbyParameters;

            m_chat = new ChatServiceBackend(owner.Log);
            m_game = new GameBackend(owner.Log);

            Initialize();
        }

        private void Initialize()
        {
            int numPlayers = m_lobbyParameters.GameFormat.NumPlayers;

            for (int i = 0; i < numPlayers; i++)
            {
                m_slots.Add(new PlayerSlot());
            }
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

        public IReadOnlyList<PlayerSlot> PlayerSlots
        {
            get { return m_slots; }
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
            UserInternalData userData = new UserInternalData(user, channel);

            lock (m_lock)
            {
                if (m_state != LobbyState.Open || m_users.Contains(channel))
                {
                    return false;
                }

                channel.MessageReceived += WhenMessageReceived;

                m_chat.Register(user, channel, ChatLevel.Normal);

                SendUserJoinMessages(channel, user);

#warning [MEDIUM] Only assign non-spectators
                AssignUserToFreeSlot(user);

                m_users.Add(channel, userData);
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
                    UnassignUserFromSlot(userData.User);

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

        public bool TryGetUser(Guid id, out User user)
        {
            return m_users.TryGetUser(id, out user);
        }

        #endregion

        #region GetLobbyDetails

        private GetLobbyDetailsResponse GetLobbyDetails(GetLobbyDetailsRequest request)
        {
            return new GetLobbyDetailsResponse
            {
                Users = new UserChangedResponse(UserChange.Joined, Users),
                Slots = new PlayerSlotChangedMessage(PlayerSlots)
            };
        }

        #endregion

        #region Slots Management

        public SetPlayerSlotDataResult SetPlayerSlotData(IChannel channel, int slotIndex, PlayerSlotData data)
        {
            if (slotIndex < 0 || slotIndex >= m_slots.Count)
            {
                return SetPlayerSlotDataResult.InvalidPlayerSlot;
            }

            // TODO: Validate the data

            lock (m_lock)
            {
                UserInternalData userData;
                if (!m_users.TryGetUser(channel, out userData))
                {
                    return SetPlayerSlotDataResult.UnauthorizedAccess;
                }

                var slot = m_slots[slotIndex];

                if (!CanSetPlayerSlotData(userData, slot))
                {
                    return SetPlayerSlotDataResult.UnauthorizedAccess;
                }

                slot.Data = data;
                m_slots[slotIndex] = slot;
                SendPlayerSlotChangedMessages(slotIndex, PlayerSlotNetworkDataChange.Data, slot);
            }

            return SetPlayerSlotDataResult.Success;
        }

        private SetPlayerSlotDataResponse SetPlayerSlotData(IChannel channel, SetPlayerSlotDataRequest request)
        {
            PlayerSlotData data;
            if (!request.Data.TryGetPlayerSlotData(out data))
            {
                return new SetPlayerSlotDataResponse { Result = SetPlayerSlotDataResult.InvalidData };
            }

            var result = SetPlayerSlotData(channel, request.Index, data);
            return new SetPlayerSlotDataResponse { Result = result };
        }

        private static bool CanSetPlayerSlotData(UserInternalData userData, PlayerSlot slot)
        {
            if (userData.User == slot.User || !slot.IsAssigned)
            {
                return true; // Can change our own player
            }

            return false;
        }

        private void AssignUserToFreeSlot(User user)
        {
            for (int i = 0; i < m_slots.Count; i++)
            {
                if (!m_slots[i].IsAssigned)
                {
                    var slot = m_slots[i];
                    slot.User = user;
                    m_slots[i] = slot;
                    SendPlayerSlotChangedMessages(i, PlayerSlotNetworkDataChange.User, slot);
                    break;
                }
            }
        }

        private void UnassignUserFromSlot(User user)
        {
            for (int i = 0; i < m_slots.Count; i++)
            {
                if (m_slots[i].User == user)
                {
                    m_slots[i] = new PlayerSlot();
                    SendPlayerSlotChangedMessages(i, PlayerSlotNetworkDataChange.All, m_slots[i]);
                }
            }
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

            public readonly User User;
            public readonly IChannel Channel;

            #endregion

            #region Constructor

            public UserInternalData(User user, IChannel channel)
            {
                User = user;
                Channel = channel;
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

            private readonly Dictionary<IChannel, UserInternalData> m_byChannel = new Dictionary<IChannel, UserInternalData>();
            private readonly Dictionary<User, UserInternalData> m_byUser = new Dictionary<User, UserInternalData>();

            #endregion

            #region Properties

            public User[] AllUsers
            {
                get { return m_byChannel.Values.Select(d => d.User).ToArray(); }
            }

            public IChannel[] Channels
            {
                get { return m_byChannel.Keys.ToArray(); }
            }

            public int Count
            {
                get { return m_byChannel.Count; }
            }

            #endregion

            #region Methods

            public bool Contains(IChannel channel)
            {
                return m_byChannel.ContainsKey(channel);
            }

            public bool TryGetUser(IChannel channel, out UserInternalData userData)
            {
                return m_byChannel.TryGetValue(channel, out userData);
            }

            public bool TryGetChannel(User user, out IChannel channel)
            {
                UserInternalData data;
                if (m_byUser.TryGetValue(user, out data))
                {
                    channel = data.Channel;
                    return true;
                }

                channel = null;
                return false;
            }

            public bool TryGetUser(Guid id, out User user)
            {
                UserInternalData data;
                if (m_byUser.TryGetValue(new User(id), out data))
                {
                    user = data.User;
                    return true;
                }

                user = new User();
                return false;
            }

            public void Add(IChannel channel, UserInternalData data)
            {
                m_byChannel.Add(channel, data);
                m_byUser.Add(data.User, data);
            }

            public bool Remove(IChannel channel, out UserInternalData data)
            {
                if (m_byChannel.TryGetValue(channel, out data))
                {
                    bool removed = m_byChannel.Remove(channel);
                    Debug.Assert(removed);

                    removed = m_byUser.Remove(data.User);
                    Debug.Assert(removed);

                    return true;
                }

                return false;
            }

            #endregion
        }

        #endregion
    }
}
