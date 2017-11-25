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

        private User m_leader;
        private readonly Bots m_bots = new Bots();
        private readonly UserCollection m_users = new UserCollection();
        private readonly PlayerSlotCollection m_slots;

        private LobbyState m_state = LobbyState.Open;
        private LobbyGameParameters m_gameParameters = new LobbyGameParameters();

        #endregion

        #region Constructor

        static LobbyBackend()
        {
            ms_router.Register<Network.Protocol.ChatMessage>(lobby => lobby.Say);
            ms_router.Register<GetLobbyDetailsRequest, GetLobbyDetailsResponse>(lobby => lobby.GetLobbyDetails);
            ms_router.Register<GetUserIdentityRequest, GetUserIdentityResponse>(lobby => lobby.GetUserIdentity);
            ms_router.Register<SetPlayerSlotDataRequest, SetPlayerSlotDataResponse>(lobby => lobby.SetPlayerSlotData);
            ms_router.Register<SetLobbyGameParametersRequest, SetLobbyGameParametersResponse>(lobby => lobby.SetGameParameters);
            ms_router.Register<StartGameRequest, StartGameResponse>(lobby => lobby.StartGame);
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

            m_gameParameters.SetAsDefault();
        }

        #endregion

        #region Properties

        public Guid Id
        {
            get { return m_id; }
        }

        public User Leader
        {
            get { return m_leader; }
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

        public IList<KeyValuePair<User, UserData>> UserDatas
        {
            get
            {
                lock (m_lock)
                {
                    return m_users.AllUserDatas;
                }
            }
        }

        internal IReadOnlyList<PlayerSlot> PlayerSlots
        {
            get
            {
                lock (m_lock)
                {
                    return m_slots.Slots;
                }
            }
        }

        public IReadOnlyList<PlayerSlotData> PlayerSlotDatas
        {
            get
            {
                lock (m_lock)
                {
                    return m_slots.ClientSlots;
                }
            }
        }

        public LobbyParameters Parameters
        {
            get { return m_lobbyParameters; }
        }

        public LobbyGameParameters GameParameters
        {
            get { return m_gameParameters; }
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

        internal bool Login(User user, IUserIdentity identity)
        {
            lock (m_lock)
            {
                if (m_state != LobbyState.Open)
                    return false;

                if (m_users.Contains(user))
                    return false;

                var lobbyUser = new LobbyUser(user, identity, false);
                SendUserJoinedMessages(lobbyUser);

                m_chat.Register(user, ChatLevel.Normal);

                if (m_lobbyParameters.AssignNewPlayersToFreeSlots)
                    m_slots.AssignToFreeSlot(lobbyUser);

                if (!m_leader.IsValid)
                {
                    ChooseNewLeader(user);
                }

                m_users.Add(lobbyUser);
            }

            return true;
        }

        public bool Logout(User user, string reason)
        {
            lock (m_lock)
            {
                if (m_users.TryGetUser(user.Id, out LobbyUser lobbyUser))
                {
                    m_users.Remove(user);
                    Log.Log(LogImportance.Normal, "{0} left lobby {1} ({2})", user, Id, reason);

                    if (m_leader == user)
                    {
                        ChooseNewLeader(m_users.FirstConnectedPlayer);
                    }

                    m_chat.Unregister(user);

                    m_slots.Unassign(lobbyUser);
                    SendUserLeftMessages(user, reason);
                }

                if (m_users.Count == 0)
                {
                    m_game.Dispose();
                    m_state = LobbyState.Closed;
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Leader

        private void ChooseNewLeader(User leader)
        {
            m_leader = leader;
            Broadcast(new LeaderChangedMessage { LeaderId = m_leader.Id });
        }

        #endregion

        #region GetLobbyDetails

        private GetLobbyDetailsResponse GetLobbyDetails(GetLobbyDetailsRequest request)
        {
            lock (m_lock)
            {
                return new GetLobbyDetailsResponse
                {
                    Users = m_users.CreateUserJoinedMessageForAllUsers(),
                    Slots = new PlayerSlotsChangedMessage(PlayerSlotDatas),
                    Leader = new LeaderChangedMessage { LeaderId = m_leader == null ? Guid.Empty : m_leader.Id },
                    GameParameters = new LobbyGameParametersChangedMessage { Parameters = m_gameParameters }
                };
            }
        }

        #endregion

        #region GetUserIdentity

        private GetUserIdentityResponse GetUserIdentity(GetUserIdentityRequest request)
        {
            lock (m_lock)
            {
                m_users.TryGetUser(request.UserId, out LobbyUser user);

                return new GetUserIdentityResponse
                {
                    Identity = user?.Identity
                };
            }
        }

        #endregion

        #region Slots Management

        public SetPlayerSlotDataResult SetPlayerSlotData(User user, int slotIndex, PlayerSlotData newSlot)
        {
            if (slotIndex < 0 || slotIndex >= m_lobbyParameters.GameFormat.NumPlayers)
            {
                return SetPlayerSlotDataResult.InvalidPlayerSlot;
            }

            PlayerSlot oldData;
            PlayerSlot current;

            lock (m_lock)
            {
                if (m_state != LobbyState.Open)
                    return SetPlayerSlotDataResult.GameAlreadyStarted;

                oldData = m_slots[slotIndex];

                if (!CheckPermissionToChangeSlotData(user, oldData))
                    return SetPlayerSlotDataResult.UnauthorizedAccess;

                current = oldData;
                CopyPlayerSlotData(ref current, newSlot);
                m_slots.Set(slotIndex, ref current);
            }

            SendPlayerSlotDataChangedMessages(user, slotIndex, oldData, current);
            return SetPlayerSlotDataResult.Success;
        }

        private SetPlayerSlotDataResponse SetPlayerSlotData(User user, SetPlayerSlotDataRequest request)
        {
            var result = SetPlayerSlotData(user, request.Index, request.Data);
            return new SetPlayerSlotDataResponse { Result = result };
        }

        private bool CheckPermissionToChangeSlotData(User user, PlayerSlot slot)
        {
            if (slot.PlayerId == user.Id)
                return true;

            if (slot.Player == null)
                return true;

            if (slot.Player.IsBot)
                return user == m_leader;

            return false;
        }

        private void CopyPlayerSlotData(ref PlayerSlot slot, PlayerSlotData newData)
        {
            m_users.TryGetUser(newData.PlayerId, out LobbyUser user);

            if (user != null && slot.Player != user)
                m_slots.Unassign(user);

            slot.Player = user;
            slot.FromClientData(newData);
        }

        private void SendPlayerSlotDataChangedMessages(User user, int slotIndex, PlayerSlot oldData, PlayerSlot newData)
        {
            if (newData.Player != oldData.Player)
            {
                if (newData.Player != null)
                {
                    BroadcastServerMessage(newData.Player, $"joined slot {slotIndex}");
                }
                else if (oldData.Player != null)
                {
                    BroadcastServerMessage(oldData.Player, $"left slot {slotIndex}");
                }
            }

            string forSlotSuffix = string.Empty;
            User origin = user;

            if (newData.Player == null)
            {
                forSlotSuffix = $" for slot {slotIndex}";
            }
            else
            {
                origin = newData.Player.User;
            }

            if (newData.Deck != oldData.Deck)
            {
                BroadcastServerMessage(origin, $"selected deck {newData.Deck.Name}" + forSlotSuffix);
            }

            if (newData.IsReady != oldData.IsReady && !newData.IsFree)
            {
                if (newData.IsReady)
                {
                    BroadcastServerMessage(origin, "is ready");
                }
                else
                {
                    BroadcastServerMessage(origin, "is not ready");
                }
            }
        }

        #endregion

        #region Chat

        private void Say(User user, Network.Protocol.ChatMessage message)
        {
            m_chat.Say(user, message.Message);
        }

        #endregion

        #region Game

        private SetLobbyGameParametersResponse SetGameParameters(User user, SetLobbyGameParametersRequest request)
        {
            lock (m_lock)
            {
                if (m_state != LobbyState.Open)
                    return new SetLobbyGameParametersResponse(false);

                if (user != m_leader)
                    return new SetLobbyGameParametersResponse(false);

                m_gameParameters = request.Parameters;
                Broadcast(new LobbyGameParametersChangedMessage { Parameters = m_gameParameters });
            }

            return new SetLobbyGameParametersResponse(true);
        }

        private StartGameResponse StartGame(User user, StartGameRequest message)
        {
            lock (m_lock)
            {
                if (m_state != LobbyState.Open)
                    return StartGameResponse_Fail();

                if (user != m_leader)
                    return StartGameResponse_Fail();

                if (!m_slots.CanStartGame())
                    return StartGameResponse_Fail();

                m_state = LobbyState.GameStarted;
            }

            m_game.StartGame(this);
            return new StartGameResponse { Result = true };
        }

        private StartGameResponse StartGameResponse_Fail()
        {
            return new StartGameResponse { Result = false };
        }

        #endregion

        #endregion

        #region Inner Types

        private enum LobbyState
        {
            Open,
            GameStarted,
            Closed
        }

        #endregion
    }
}
