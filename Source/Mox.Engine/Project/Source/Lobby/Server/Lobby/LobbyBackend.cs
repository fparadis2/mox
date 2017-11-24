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

        public IReadOnlyList<PlayerSlotData> PlayerSlots
        {
            get { return m_slots; }
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

                SendUserJoinedMessages(user, new UserData { Name = identity.Name });

                m_chat.Register(user, ChatLevel.Normal);
                m_slots.AssignPlayerToFreeSlot(user);

                if (!m_leader.IsValid)
                {
                    ChooseNewLeader(user);
                }

                m_users.Add(user, identity);
            }

            return true;
        }

        public bool Logout(User user, string reason)
        {
            lock (m_lock)
            {
                if (m_users.Remove(user))
                {
                    Log.Log(LogImportance.Normal, "{0} left lobby {1} ({2})", user, Id, reason);

                    if (m_leader == user)
                    {
                        ChooseNewLeader(m_users.FirstConnectedPlayer);
                    }

                    m_chat.Unregister(user);

                    m_slots.UnassignPlayerFromSlot(user);
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

        internal bool TryGetUser(Guid id, out User user, out IUserIdentity identity)
        {
            lock (m_lock)
            {
                return m_users.TryGetUser(id, out user, out identity);
            }
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
                    Slots = new PlayerSlotsChangedMessage(PlayerSlots),
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
                m_users.TryGetUser(request.UserId, out User user, out IUserIdentity identity);

                return new GetUserIdentityResponse
                {
                    Identity = identity
                };
            }
        }

        #endregion

        #region Slots Management

        public SetPlayerSlotDataResult SetPlayerSlotData(User user, int slotIndex, PlayerSlotDataMask mask, PlayerSlotData newSlot)
        {
            if (slotIndex < 0 || slotIndex >= m_slots.Count)
            {
                return SetPlayerSlotDataResult.InvalidPlayerSlot;
            }

            if (mask == PlayerSlotDataMask.None)
                return SetPlayerSlotDataResult.Success;

            PlayerSlotData oldData;
            PlayerSlotData current;

            lock (m_lock)
            {
                if (m_state != LobbyState.Open)
                    return SetPlayerSlotDataResult.GameAlreadyStarted;

                oldData = m_slots[slotIndex];

                current = oldData;
                if (current.IsAssigned && current.PlayerId != user.Id)
                {
                    // Owned by someone else
                    return SetPlayerSlotDataResult.UnauthorizedAccess;
                }

                CopyPlayerSlotData(ref current, user, mask, newSlot);
                m_slots.Set(slotIndex, ref current);
            }

            SendPlayerSlotDataChangedMessages(user, slotIndex, mask, oldData, current);
            return SetPlayerSlotDataResult.Success;
        }

        private SetPlayerSlotDataResponse SetPlayerSlotData(User user, SetPlayerSlotDataRequest request)
        {
            var result = SetPlayerSlotData(user, request.Index, request.Mask, request.Data);
            return new SetPlayerSlotDataResponse { Result = result };
        }

        private void CopyPlayerSlotData(ref PlayerSlotData data, User user, PlayerSlotDataMask mask, PlayerSlotData newData)
        {
            if (mask.HasFlag(PlayerSlotDataMask.PlayerId))
            {
                if (newData.IsAssigned && newData.PlayerId != data.PlayerId)
                {
                    m_slots.UnassignPlayerFromSlot(user);
                }

                data.PlayerId = newData.PlayerId;
            }

            if (mask.HasFlag(PlayerSlotDataMask.Deck))
            {
                data.Deck = newData.Deck;
            }

            if (mask.HasFlag(PlayerSlotDataMask.Ready))
            {
                data.IsReady = newData.IsReady;
            }
        }

        private void SendPlayerSlotDataChangedMessages(User user, int slotIndex, PlayerSlotDataMask mask, PlayerSlotData oldData, PlayerSlotData newData)
        {
            if (mask.HasFlag(PlayerSlotDataMask.PlayerId))
            {
                if (newData.PlayerId != oldData.PlayerId)
                {
                    if (newData.IsAssigned)
                    {
                        BroadcastServerMessage(user, $"joined slot {slotIndex}");
                    }
                    else
                    {
                        BroadcastServerMessage(user, $"left slot {slotIndex}");
                    }
                }
            }

            if (mask.HasFlag(PlayerSlotDataMask.Deck))
            {
                BroadcastServerMessage(user, $"selected deck {newData.Deck.Name}");
            }

            if (mask.HasFlag(PlayerSlotDataMask.Ready))
            {
                if (newData.IsReady != oldData.IsReady)
                {
                    if (newData.IsReady)
                    {
                        BroadcastServerMessage(user, "is ready");
                    }
                    else
                    {
                        BroadcastServerMessage(user, "is not ready");
                    }
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

                foreach (var slot in m_slots)
                {
                    if (slot.IsReady)
                        continue;

                    if (slot.PlayerId == user.Id && slot.IsValid)
                        continue; // Leader is considered ready if valid

                    return StartGameResponse_Fail();
                }

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
