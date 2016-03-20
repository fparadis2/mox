using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Mox.Lobby.Network;
using Mox.Lobby.Network.Protocol;

namespace Mox.Lobby.Server
{
    public partial class LobbyBackend
    {
        #region Variables

        private static readonly MessageRouter<LobbyBackend, User> ms_router = new MessageRouter<LobbyBackend, User>();

        private readonly LobbyParameters m_lobbyParameters;
        private readonly LobbyServiceBackend m_owner;
        private readonly ChatServiceBackend m_chat;
        private readonly GameBackend m_game;

        private readonly Guid m_id = Guid.NewGuid();
        private readonly object m_lock = new object();

        private readonly UserCollection m_users = new UserCollection();
        private readonly PlayerSlotCollection m_slots;

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

            m_slots = new PlayerSlotCollection(this);

            m_chat = new ChatServiceBackend(owner.Log);
            m_game = new GameBackend(owner.Log);
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

        public IList<PlayerData> PlayerDatas
        {
            get
            {
                lock (m_lock)
                {
                    return m_users.AllPlayerDatas;
                }
            }
        }

        public IReadOnlyList<PlayerSlotData> PlayerSlots
        {
            get { return m_slots; }
        }

        public ILog Log
        {
            get { return m_owner.Log; }
        }

        #endregion

        #region Methods

        #region Routing

        internal Response Route(User user, Message message)
        {
            return ms_router.Route(this, user, message);
        }

        #endregion

        #region User Management

        internal bool Login(User user)
        {
            lock (m_lock)
            {
                if (m_state != LobbyState.Open)
                    return false;

                if (m_users.Contains(user))
                    return false;

                PlayerData data = new PlayerData(user.Id, user.Name);
                m_chat.Register(user, ChatLevel.Normal);

                SendPlayerJoinMessages(user, data);
                m_slots.AssignPlayerToFreeSlot(user);

                m_users.Add(user, data);
            }

            return true;
        }

        public bool Logout(User user, string reason)
        {
            lock (m_lock)
            {
                PlayerData data;
                if (m_users.Remove(user, out data))
                {
                    Log.Log(LogImportance.Normal, "{0} left lobby {1} ({2})", user, Id, reason);

                    m_chat.Unregister(user);

                    m_slots.UnassignPlayerFromSlot(user);
                    SendPlayerLeaveMessages(user, data, reason);
                }

                if (m_users.Count == 0)
                {
                    m_state = LobbyState.Closed;
                    return true;
                }
            }

            return false;
        }

        internal bool TryGetPlayer(Guid id, out User user, out PlayerData playerData)
        {
            lock (m_lock)
            {
                return m_users.TryGetPlayer(id, out user, out playerData);
            }
        }

        #endregion

        #region GetLobbyDetails

        private GetLobbyDetailsResponse GetLobbyDetails(GetLobbyDetailsRequest request)
        {
            return new GetLobbyDetailsResponse
            {
                Players = new PlayersChangedMessage(PlayersChangedMessage.ChangeType.Joined, PlayerDatas),
                Slots = new PlayerSlotsChangedMessage(PlayerSlots)
            };
        }

        #endregion

        #region Slots Management

        public SetPlayerSlotDataResult SetPlayerSlotData(User user, int slotIndex, PlayerSlotData newSlot)
        {
            if (slotIndex < 0 || slotIndex >= m_slots.Count)
            {
                return SetPlayerSlotDataResult.InvalidPlayerSlot;
            }

            lock (m_lock)
            {
                var current = m_slots[slotIndex];

                var result = HandleSlotAssignment(user, current, newSlot);
                if (result != SetPlayerSlotDataResult.Success)
                    return result;

                m_slots.Set(slotIndex, ref newSlot);
            }

            return SetPlayerSlotDataResult.Success;
        }

        private SetPlayerSlotDataResponse SetPlayerSlotData(User user, SetPlayerSlotDataRequest request)
        {
            var result = SetPlayerSlotData(user, request.Index, request.Data);
            return new SetPlayerSlotDataResponse { Result = result };
        }

        private SetPlayerSlotDataResult HandleSlotAssignment(User user, PlayerSlotData current, PlayerSlotData newSlot)
        {
            if (current.IsAssigned && current.PlayerId != user.Id)
            {
                // Owned by someone else
                return SetPlayerSlotDataResult.UnauthorizedAccess;
            }

            if (current.PlayerId == newSlot.PlayerId)
            {
                // Same player
                return SetPlayerSlotDataResult.Success;
            }

            if (!newSlot.IsAssigned)
            {
                // Unassignment is ok
                return SetPlayerSlotDataResult.Success;
            }

            // Assignment requires unassigning first
            m_slots.UnassignPlayerFromSlot(user);
            return SetPlayerSlotDataResult.Success;
        }

        #endregion

        #region Chat

        private void Say(User user, ChatMessage message)
        {
            m_chat.Say(user, message.Message);
        }

        #endregion

        #region Game

        private void StartGame(User user, StartGameRequest message)
        {
#warning [Medium] TODO: Check that the starter is the lobby leader
#warning [Medium] TODO: Check that everyone is ready
            m_game.StartGame(this);
        }

        #endregion

        #endregion

        #region Inner Types

        private enum LobbyState
        {
            Open,
            Closed
        }

        private class UserCollection
        {
            #region Variables

            private readonly KeyedUserCollection m_players = new KeyedUserCollection();

            #endregion

            #region Properties

            public User[] AllUsers
            {
                get { return m_players.Select(u => u.User).ToArray(); }
            }

            public PlayerData[] AllPlayerDatas
            {
                get { return m_players.Select(u => u.Data).ToArray(); }
            }

            public int Count
            {
                get { return m_players.Count; }
            }

            #endregion

            #region Methods

            public bool Contains(User user)
            {
                return m_players.Contains(user.Id);
            }

            public void Add(User user, PlayerData data)
            {
                Debug.Assert(data.Id == user.Id);

                PlayerInfo playerInfo = new PlayerInfo(user, data);
                m_players.Add(playerInfo);
            }

            public bool Remove(User user, out PlayerData data)
            {
                PlayerInfo playerInfo;
                if (m_players.TryGetValue(user.Id, out playerInfo))
                {
                    m_players.Remove(user.Id);
                    data = playerInfo.Data;
                    return true;
                }

                data = new PlayerData();
                return false;
            }

            public bool TryGetPlayer(Guid id, out User user, out PlayerData playerData)
            {
                PlayerInfo playerInfo;
                if (m_players.TryGetValue(id, out playerInfo))
                {
                    user = playerInfo.User;
                    playerData = playerInfo.Data;
                    return true;
                }

                user = null;
                playerData = new PlayerData();
                return false;
            }

            #endregion

            #region Nested Types

            private class PlayerInfo
            {
                public readonly User User;
                public PlayerData Data;

                public PlayerInfo(User user, PlayerData data)
                {
                    Throw.IfNull(user, "user");
                    User = user;
                    Data = data;
                }
            }

            private class KeyedUserCollection : KeyedCollection<Guid, PlayerInfo>
            {
                protected override Guid GetKeyForItem(PlayerInfo item)
                {
                    return item.User.Id;
                }
            }

            #endregion
        }

        #endregion
    }
}
