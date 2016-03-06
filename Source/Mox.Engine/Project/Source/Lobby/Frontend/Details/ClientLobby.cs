using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using Mox.Database;

namespace Mox.Lobby
{
    internal class ClientLobby : IDisposable, ILobby, IChatService, IServerMessages
    {
        #region Variables

        private static readonly MessageRouter<ClientLobby> ms_router = new MessageRouter<ClientLobby>();

        private readonly IChannel m_channel;

        private readonly UserCollection m_users = new UserCollection();
        private readonly PlayerSlotCollection m_slots = new PlayerSlotCollection();
        private readonly ClientGame m_game;

        private User m_user;
        private Guid m_lobbyId;

        #endregion

        #region Constructor

        static ClientLobby()
        {
            ms_router.Register<UserChangedResponse>(c => c.OnUserChanged);
            ms_router.Register<PlayerSlotChangedMessage>(c => c.OnPlayerSlotChanged);
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

        public User User
        {
            get { return m_user; }
        }

        public ILobbyItemCollection<User> Users
        {
            get { return m_users.AsReadOnly(); }
        }

        public IPlayerSlotCollection Slots
        {
            get { return m_slots; }
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

        #region Slots

        Task<SetPlayerSlotDataResult> ILobby.SetPlayerSlotData(int slotIndex, PlayerSlotData data)
        {
            return SetPlayerSlotData(slotIndex, data);
        }

        private async Task<SetPlayerSlotDataResult> SetPlayerSlotData(int slotIndex, PlayerSlotData data)
        {
            SetPlayerSlotDataRequest request = new SetPlayerSlotDataRequest
            {
                Index = slotIndex,
                Data = new PlayerSlotNetworkData(data),
            };

            var response = await m_channel.Request<SetPlayerSlotDataRequest, SetPlayerSlotDataResponse>(request);
            return response.Result;
        }

        Task<AssignPlayerSlotResult> ILobby.AssignPlayerSlot(int slotIndex, User user)
        {
            return AssignPlayerSlot(slotIndex, user);
        }

        private async Task<AssignPlayerSlotResult> AssignPlayerSlot(int slotIndex, User user)
        {
            AssignPlayerSlotRequest request = new AssignPlayerSlotRequest
            {
                Index = slotIndex,
                User = user.Id
            };

            var response = await m_channel.Request<AssignPlayerSlotRequest, AssignPlayerSlotResponse>(request);
            return response.Result;
        }
        
        #endregion

        #endregion

        #region Methods

        internal void Initialize(JoinLobbyResponse response)
        {
            m_user = response.User;
            m_lobbyId = response.LobbyId;

            m_game.User = response.User;

            m_slots.Clear();
            for (int i = 0; i < response.NumSlots; i++)
                m_slots.Add(new PlayerSlot());

            UpdateLobby();
        }

        private void UpdateLobby()
        {
            GetLobbyDetailsRequest request = new GetLobbyDetailsRequest();
            var response = m_channel.Request<GetLobbyDetailsRequest, GetLobbyDetailsResponse>(request).Result;
            OnUserChanged(response.Users);
            OnPlayerSlotChanged(response.Slots);
        }

        private void WhenMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            ms_router.Route(this, m_channel, e.Message);
            m_game.ReceiveMessage(e.Message);
        }

        public void Say(string msg)
        {
            var message = new ChatMessage { Speaker = m_user.Id, Message = msg };
            m_channel.Send(message);
            OnChatMessage(message);
        }

        private void OnChatMessage(ChatMessage message)
        {
            User speaker;
            if (m_users.TryGetValue(message.Speaker, out speaker))
            {
                ChatMessageReceived.Raise(this, new ChatMessageReceivedEventArgs(speaker, message.Message));
            }
        }

        private void OnServerMessage(ServerMessage message)
        {
            User user;
            m_users.TryGetValue(message.User, out user);
            ServerMessageReceived.Raise(this, new ServerMessageReceivedEventArgs(user, message.Message));
        }

        private void OnUserChanged(UserChangedResponse response)
        {
            switch (response.Change)
            {
                case UserChange.Joined:
                    foreach (var user in response.Users)
                    {
                        m_users.Add(user);
                    }
                    break;

                case UserChange.Left:
                    foreach (var user in response.Users)
                    {
                        m_users.Remove(user);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void OnPlayerSlotChanged(PlayerSlotChangedMessage message)
        {
            foreach (var change in message.Changes)
            {
                OnPlayerSlotChanged(change);
            }
        }

        private void OnPlayerSlotChanged(PlayerSlotChangedMessage.Change change)
        {
            bool changed = false;

            if (change.Type.HasFlag(PlayerSlotNetworkDataChange.User))
            {
                User user;
                m_users.TryGetValue(change.SlotData.User, out user);

                m_slots.SetSlot(change.Index, s =>
                {
                    s.User = user;
                    return s;
                });

                changed = true;
            }

            if (change.Type.HasFlag(PlayerSlotNetworkDataChange.Deck))
            {
                IDeck deck;
                change.SlotData.Deck.TryGetDeck(out deck);

                m_slots.SetSlot(change.Index, s =>
                {
                    var data = s.Data;
                    data.Deck = deck;
                    s.Data = data;
                    return s;
                });

                changed = true;
            }

            if (changed)
            {
                m_slots.OnItemChanged(change.Index);
            }
        }

        #endregion

        #region Events

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

        #region Inner Types

        private class UserCollection : LobbyItemCollection<User>
        {
            public bool TryGetValue(Guid id, out User user)
            {
                foreach (var element in this)
                {
                    if (element.Id == id)
                    {
                        user = element;
                        return true;
                    }
                }

                user = new User();
                return false;
            }
        }

        private class PlayerSlotCollection : List<PlayerSlot>, IPlayerSlotCollection
        {
            public void SetSlot(int index, Func<PlayerSlot, PlayerSlot> callback)
            {
                var slot = this[index];
                slot = callback(slot);
                this[index] = slot;
            }

            public event EventHandler<ItemEventArgs<int>> ItemChanged;

            public void OnItemChanged(int index)
            {
                ItemChanged.Raise(this, new ItemEventArgs<int>(index));
            }
        }

        private class LobbyItemCollection<T> : Collections.ObservableCollection<T>, ILobbyItemCollection<T>
        {
            #region Methods

            public new ILobbyItemCollection<T> AsReadOnly()
            {
                return (ILobbyItemCollection<T>)base.AsReadOnly();
            }

            protected override IObservableCollection<T> CreateReadOnlyWrapper()
            {
                return new ReadOnlyWrapper(this);
            }

            #endregion

            #region Events

            public event EventHandler<ItemEventArgs<T>> ItemChanged;

            protected virtual void OnItemChanged(ItemEventArgs<T> e)
            {
                ItemChanged.Raise(this, e);
            }

            #endregion

            #region Inner Types

            private class ReadOnlyWrapper : ReadOnlyObservableCollection, ILobbyItemCollection<T>
            {
                public ReadOnlyWrapper(ILobbyItemCollection<T> collection)
                    : base(collection)
                {
                    collection.ItemChanged += collection_ItemChanged;

                }

                #region Events

                void collection_ItemChanged(object sender, ItemEventArgs<T> e)
                {
                    ItemChanged.Raise(this, e);
                }

                public event EventHandler<ItemEventArgs<T>> ItemChanged;

                #endregion
            }

            #endregion
        }

        #endregion
    }
}