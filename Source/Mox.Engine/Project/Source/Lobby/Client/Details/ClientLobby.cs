using System;
using System.Threading.Tasks;

using Mox.Lobby.Network;
using Mox.Lobby.Network.Protocol;

namespace Mox.Lobby.Client
{
    internal class ClientLobby : IDisposable, ILobby, IMessageService
    {
        #region Variables

        private static readonly MessageRouter<ClientLobby, IChannel> ms_router = new MessageRouter<ClientLobby, IChannel>();

        private readonly IChannel m_channel;

        private readonly ClientUserCollection m_users = new ClientUserCollection();
        private readonly ClientPlayerSlotCollection m_slots = new ClientPlayerSlotCollection();
        private readonly ClientGame m_game;

        private LobbyParameters m_lobbyParameters;
        private LobbyGameParameters m_gameParameters;
        private Guid m_localUserId;
        private Guid m_lobbyId;
        private Guid m_leaderId;
        private IUserIdentity m_localIdentity;

        #endregion

        #region Constructor

        static ClientLobby()
        {
            ms_router.Register<UserJoinedMessage>(c => c.m_users.HandleMessage);
            ms_router.Register<UserLeftMessage>(c => c.m_users.HandleMessage);

            ms_router.Register<PlayerSlotsChangedMessage>(c => c.m_slots.HandleChangedMessage);
            ms_router.Register<LeaderChangedMessage>(c => c.HandleLeaderChangedMessage);
            ms_router.Register<LobbyGameParametersChangedMessage>(c => c.HandleLobbyGameParametersChangedMessage);
            ms_router.Register<Network.Protocol.ChatMessage>(c => c.OnChatMessage);
            ms_router.Register<Network.Protocol.ServerMessage>(c => c.OnServerMessage);
        }

        public ClientLobby(IChannel channel)
        {
            Throw.IfNull(channel, "client");
            m_channel = channel;
            m_game = new ClientGame(channel);
            m_channel.MessageReceived += WhenMessageReceived;
        }

        public void Dispose()
        {
            m_channel.MessageReceived -= WhenMessageReceived;
        }

        #endregion

        #region Properties

        public Guid Id
        {
            get { return m_lobbyId; }
        }

        public Guid LocalUserId
        {
            get { return m_localUserId; }
        }

        public Guid LeaderId
        {
            get { return m_leaderId; }
        }

        public ILobbyUserCollection Users
        {
            get { return m_users; }
        }

        public IPlayerSlotCollection Slots
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

        public IMessageService Messages
        {
            get { return this; }
        }

        public IGameService GameService
        {
            get { return m_game; }
        }

        public bool IsLoggedIn
        {
            get { return m_lobbyId != Guid.Empty; }
        }

        #endregion

        #region Methods

        internal void Initialize(JoinLobbyResponse response, IUserIdentity localIdentity)
        {
            string error;
            m_lobbyParameters = response.LobbyParameters.ToParameters(out error);
            if (!string.IsNullOrEmpty(error))
                throw new InvalidOperationException(string.Format("Unsupported lobby parameters: {0}", error));

            m_localUserId = response.UserId;
            m_lobbyId = response.LobbyId;
            m_localIdentity = localIdentity;

            m_game.LocalUserId = response.UserId;

            m_slots.Clear();
            for (int i = 0; i < response.NumSlots; i++)
                m_slots.Add(new PlayerSlotData());

            UpdateLobby();
        }

        private void UpdateLobby()
        {
            GetLobbyDetailsRequest request = new GetLobbyDetailsRequest();
            var response = m_channel.Request<GetLobbyDetailsRequest, GetLobbyDetailsResponse>(request).Result;

            foreach (var userJoined in response.Users)
                m_users.HandleMessage(userJoined);

            m_slots.HandleChangedMessage(response.Slots);
            HandleLeaderChangedMessage(response.Leader);
            HandleLobbyGameParametersChangedMessage(response.GameParameters);
        }

        private void HandleLeaderChangedMessage(LeaderChangedMessage msg)
        {
            if (m_leaderId != msg.LeaderId)
            {
                m_leaderId = msg.LeaderId;
                LeaderChanged.Raise(this, EventArgs.Empty);
            }
        }

        private void HandleLobbyGameParametersChangedMessage(LobbyGameParametersChangedMessage msg)
        {
            m_gameParameters = msg.Parameters;
            GameParametersChanged.Raise(this, EventArgs.Empty);
        }

        private void WhenMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            ms_router.Route(this, m_channel, e.Message);
            m_game.ReceiveMessage(e.Message);
        }
        
        #region Players & Slots

        Task<SetPlayerSlotDataResult> ILobby.SetPlayerSlotData(int slotIndex, PlayerSlotData data)
        {
            return SetPlayerSlotData(slotIndex, data);
        }

        private async Task<SetPlayerSlotDataResult> SetPlayerSlotData(int slotIndex, PlayerSlotData data)
        {
            SetPlayerSlotDataRequest request = new SetPlayerSlotDataRequest
            {
                Index = slotIndex,
                Data = data,
            };

            var response = await m_channel.Request<SetPlayerSlotDataRequest, SetPlayerSlotDataResponse>(request);
            return response.Result;
        }

        #endregion

        #region User Identity

        Task<IUserIdentity> ILobby.GetUserIdentity(Guid userId)
        {
            return GetUserIdentity(userId);
        }

        private async Task<IUserIdentity> GetUserIdentity(Guid userId)
        {
            if (userId == m_localUserId)
                return m_localIdentity;

            GetUserIdentityRequest request = new GetUserIdentityRequest { UserId = userId };
            var response = await m_channel.Request<GetUserIdentityRequest, GetUserIdentityResponse>(request);
            return response.Identity;
        }

        #endregion

        #region Game Parameters

        Task<bool> ILobby.SetGameParameters(LobbyGameParameters parameters)
        {
            return SetGameParameters(parameters);
        }

        private async Task<bool> SetGameParameters(LobbyGameParameters parameters)
        {
            if (m_localUserId != m_leaderId)
                throw new InvalidOperationException("Only the lobby leader can set the game parameters.");

            SetLobbyGameParametersRequest request = new SetLobbyGameParametersRequest { Parameters = parameters };
            var response = await m_channel.Request<SetLobbyGameParametersRequest, SetLobbyGameParametersResponse>(request);
            return response.Result;
        }

        #endregion

        #region IMessageService

        void IMessageService.SendMessage(string msg)
        {
            var message = new Network.Protocol.ChatMessage { SpeakerId = m_localUserId, Message = msg };
            m_channel.Send(message);
            OnChatMessage(message);
        }

        private EventHandler<ChatMessage> ChatMessageReceived;
        event EventHandler<ChatMessage> IMessageService.ChatMessageReceived
        {
            add { ChatMessageReceived += value; }
            remove { ChatMessageReceived -= value; }
        }

        private EventHandler<ServerMessage> ServerMessageReceived;
        event EventHandler<ServerMessage> IMessageService.ServerMessageReceived
        {
            add { ServerMessageReceived += value; }
            remove { ServerMessageReceived -= value; }
        }

        private EventHandler<GameMessage> GameMessageReceived;
        event EventHandler<GameMessage> IMessageService.GameMessageReceived
        {
            add { GameMessageReceived += value; }
            remove { GameMessageReceived -= value; }
        }

        private void OnChatMessage(Network.Protocol.ChatMessage message)
        {
            ChatMessage clientMessage = new ChatMessage
            {
                SpeakerUserId = message.SpeakerId,
                Text = message.Message
            };

            ChatMessageReceived.Raise(this, clientMessage);
        }

        private void OnServerMessage(Network.Protocol.ServerMessage message)
        {
            ServerMessage clientMessage = new ServerMessage
            {
                UserId = message.User,
                Text = message.Message
            };

            ServerMessageReceived.Raise(this, clientMessage);
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler LeaderChanged;
        public event EventHandler GameParametersChanged;

        #endregion
    }
}