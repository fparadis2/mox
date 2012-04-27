using System;

namespace Mox.Lobby
{
    internal class ClientLobby : IDisposable, ILobby, IChatService
    {
        #region Variables

        private static readonly MessageRouter<ClientLobby> ms_router = new MessageRouter<ClientLobby>();

        private readonly IChannel m_channel;

        private readonly UserCollection m_users = new UserCollection();
        private readonly PlayerCollection m_players = new PlayerCollection();

        private User m_user;
        private Guid m_lobbyId;

        #endregion

        #region Constructor

        static ClientLobby()
        {
            ms_router.Register<UserChangedResponse>(c => c.OnUserChanged);
            ms_router.Register<PlayerChangedResponse>(c => c.OnPlayerChanged);
            ms_router.Register<ChatMessage>(c => c.OnChatMessage);
        }

        public ClientLobby(IChannel channel)
        {
            Throw.IfNull(channel, "client");
            m_channel = channel;
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

        public IObservableCollection<User> Users
        {
            get { return m_users.AsReadOnly(); }
        }

        public IObservableCollection<Player> Players
        {
            get { return m_players.AsReadOnly(); }
        }

        public IChatService Chat
        {
            get { return this; }
        }

        public bool IsLoggedIn
        {
            get { return m_lobbyId != Guid.Empty; }
        }

        #region Players

        SetPlayerDataResult ILobby.SetPlayerData(Guid playerId, PlayerData player)
        {
            var response = m_channel.Request<SetPlayerDataResponse>(new SetPlayerDataRequest { PlayerId = playerId, PlayerData = player });
            return response.Result;
        }
        
        #endregion

        #endregion

        #region Methods

        internal void Initialize(User user, Guid lobbyId)
        {
            m_user = user;
            m_lobbyId = lobbyId;
        }

        private void WhenMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            ms_router.Route(this, m_channel, e.Message);
        }

        public void Say(string msg)
        {
            var message = new ChatMessage { User = m_user, Message = msg };
            m_channel.Send(message);
            OnChatMessage(message);
        }

        private void OnChatMessage(ChatMessage message)
        {
            MessageReceived.Raise(this, new ChatMessageReceivedEventArgs(message.User, message.Message));
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

        private void OnPlayerChanged(PlayerChangedResponse response)
        {
            switch (response.Change)
            {
                case PlayerChange.Joined:
                    foreach (var player in response.Players)
                    {
                        m_players.Add(player);
                    }
                    break;

                case PlayerChange.Left:
                    foreach (var player in response.Players)
                    {
                        m_players.Remove(player);
                    }
                    break;

                case PlayerChange.Changed:
                    foreach (var player in response.Players)
                    {
                        m_players.Replace(player);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #region Events

        public event EventHandler<ChatMessageReceivedEventArgs> MessageReceived;

        #endregion

        #region Inner Types

        private class UserCollection : Collections.ObservableCollection<User>
        {
        }

        private class PlayerCollection : Collections.ObservableCollection<Player>
        {
            public void Replace(Player player)
            {
                foreach (var p in this)
                {
                    if (player.Id == p.Id)
                    {
                        Remove(p);
                        break;
                    }
                }

                Add(player);
            }
        }

        #endregion
    }
}
