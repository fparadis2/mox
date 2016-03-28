﻿using System;
using System.Threading.Tasks;

using Mox.Lobby.Network;
using Mox.Lobby.Network.Protocol;

namespace Mox.Lobby.Client
{
    internal class ClientLobby : IDisposable, ILobby, IChatService, IServerMessages
    {
        #region Variables

        private static readonly MessageRouter<ClientLobby, IChannel> ms_router = new MessageRouter<ClientLobby, IChannel>();

        private readonly IChannel m_channel;

        private readonly ClientPlayerCollection m_players = new ClientPlayerCollection();
        private readonly ClientPlayerSlotCollection m_slots = new ClientPlayerSlotCollection();
        private readonly ClientGame m_game;

        private LobbyParameters m_lobbyParameters;
        private Guid m_localUserId;
        private Guid m_lobbyId;
        private Guid m_leaderId;
        private IPlayerIdentity m_localIdentity;

        #endregion

        #region Constructor

        static ClientLobby()
        {
            ms_router.Register<PlayersChangedMessage>(c => c.m_players.HandleChangedMessage);
            ms_router.Register<PlayerSlotsChangedMessage>(c => c.m_slots.HandleChangedMessage);
            ms_router.Register<LeaderChangedMessage>(c => c.HandleLeaderChangedMessage);
            ms_router.Register<ChatMessage>(c => c.OnChatMessage);
            ms_router.Register<ServerMessage>(c => c.OnServerMessage);
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

        public IPlayerCollection Players
        {
            get { return m_players; }
        }

        public IPlayerSlotCollection Slots
        {
            get { return m_slots; }
        }

        public LobbyParameters Parameters
        {
            get { return m_lobbyParameters; }
        }

        public IChatService Chat
        {
            get { return this; }
        }

        public IServerMessages ServerMessages
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

        internal void Initialize(JoinLobbyResponse response, IPlayerIdentity localIdentity)
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
            m_players.HandleChangedMessage(response.Players);
            m_slots.HandleChangedMessage(response.Slots);
            HandleLeaderChangedMessage(response.Leader);
        }

        private void HandleLeaderChangedMessage(LeaderChangedMessage msg)
        {
            if (m_leaderId != msg.LeaderId)
            {
                m_leaderId = msg.LeaderId;
                LeaderChanged.Raise(this, EventArgs.Empty);
            }
        }

        private void WhenMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            ms_router.Route(this, m_channel, e.Message);
            m_game.ReceiveMessage(e.Message);
        }

        public void Say(string msg)
        {
            var message = new ChatMessage { SpeakerId = m_localUserId, Message = msg };
            m_channel.Send(message);
            OnChatMessage(message);
        }

        private void OnChatMessage(ChatMessage message)
        {
            PlayerData playerSpeaker;
            if (m_players.TryGet(message.SpeakerId, out playerSpeaker))
            {
                ChatMessageReceived.Raise(this, new ChatMessageReceivedEventArgs(playerSpeaker, message.Message));
            }
        }

        private void OnServerMessage(ServerMessage message)
        {
            ServerMessageReceived.Raise(this, new ServerMessageReceivedEventArgs(message.Message));
        }

        #region Players & Slots

        Task<SetPlayerDataResult> ILobby.SetPlayerData(PlayerData data)
        {
            return SetPlayerData(data);
        }

        private async Task<SetPlayerDataResult> SetPlayerData(PlayerData data)
        {
            SetPlayerDataRequest request = new SetPlayerDataRequest { Data = data };
            var response = await m_channel.Request<SetPlayerDataRequest, SetPlayerDataResponse>(request);
            return response.Result;
        }

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

        #region Player Identity

        Task<IPlayerIdentity> ILobby.GetPlayerIdentity(Guid playerId)
        {
            return GetPlayerIdentity(playerId);
        }

        private async Task<IPlayerIdentity> GetPlayerIdentity(Guid playerId)
        {
            if (playerId == m_localUserId)
                return m_localIdentity;

            GetPlayerIdentityRequest request = new GetPlayerIdentityRequest { PlayerId = playerId };
            var response = await m_channel.Request<GetPlayerIdentityRequest, GetPlayerIdentityResponse>(request);
            return response.Identity;
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler LeaderChanged;

        private event EventHandler<ChatMessageReceivedEventArgs> ChatMessageReceived;

        event EventHandler<ChatMessageReceivedEventArgs> IChatService.MessageReceived
        {
            add { ChatMessageReceived += value; }
            remove { ChatMessageReceived -= value; }
        }

        private event EventHandler<ServerMessageReceivedEventArgs> ServerMessageReceived;

        event EventHandler<ServerMessageReceivedEventArgs> IServerMessages.MessageReceived
        {
            add { ServerMessageReceived += value; }
            remove { ServerMessageReceived -= value; }
        }

        #endregion
    }
}