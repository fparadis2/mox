using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Castle.Core.Interceptor;

using Mox.Lobby.Network;

namespace Mox.Lobby
{
    public abstract class Client
    {
        #region Variables

        private readonly object m_lock = new object();
        private readonly FrontEnd m_frontEnd;
        private IServerContract m_server;

        private ClientState m_state = ClientState.New;

        #endregion

        #region Constructor

        protected Client()
        {
            m_frontEnd = CreateFrontEnd(this);
        }

        protected virtual FrontEnd CreateFrontEnd(Client client)
        {
            return new FrontEnd(client);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Whether the client is currently connected.
        /// </summary>
        public bool IsConnected
        {
            get { return m_state == ClientState.Connected; }
        }

        /// <summary>
        /// Lobby to which the client is connected.
        /// </summary>
        public ILobby Lobby
        {
            get
            {
                ThrowIfNotLoggedIn();
                return m_frontEnd;
            }
        }

        #endregion

        #region Methods

        #region Initialization

        protected abstract IServerContract CreateServer(IClientContract client);

        protected virtual void DeleteServerImpl()
        {
            m_frontEnd.Disconnect();
            m_server = null;
        }

        protected void DeleteServer()
        {
            bool disconnected;

            lock (m_lock)
            {
                disconnected = m_state == ClientState.Connected;
                if (disconnected)
                {
                    m_state = ClientState.Disconnected;
                    DeleteServerImpl();
                }
            }

            if (disconnected)
            {
                OnDisconnected(EventArgs.Empty);
            }
        }

        #endregion

        #region Connection

        /// <summary>
        /// Connects to the server.
        /// </summary>
        /// <remarks>
        /// True if connection could be established.
        /// </remarks>
        public bool Connect()
        {
            if (m_state == ClientState.New)
            {
                lock (m_lock)
                {
                    if (m_state == ClientState.New)
                    {
                        if (m_server == null)
                        {
                            m_server = CreateServer(m_frontEnd);
                            if (m_server != null)
                            {
                                m_state = ClientState.Connected;
                            }
                        }
                    }
                }
            }

            return m_state == ClientState.Connected;
        }

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        public void Disconnect()
        {
            if (m_state == ClientState.Connected)
            {
                lock (m_lock)
                {
                    if (m_state == ClientState.Connected)
                    {
                        TryDo(s => s.Logout());
                        DeleteServer();
                    }
                }
            }
        }

        #endregion

        #region Operations

        private void TryDo(Action<IServerContract> operation)
        {
            TryDoAndReturn(server =>
            {
                operation(m_server);
                return true;
            });
        }

        private T TryDoAndReturn<T>(Func<IServerContract, T> operation, T defaultValue = default(T))
        {
            if (m_state == ClientState.Connected)
            {
                var server = m_server;

                if (server != null)
                {
                    try
                    {
                        return operation(m_server);
                    }
                    catch
                    {
                        DeleteServer();
                        m_state = ClientState.Faulted;
                    }
                }
            }

            return defaultValue;
        }

        #endregion

        #region Login

        public IEnumerable<Guid> GetLobbies()
        {
            return TryDoAndReturn(s => s.GetLobbies(), Enumerable.Empty<Guid>());
        }

        public void CreateLobby(string username)
        {
            ThrowIfLoggedIn();

            LoginDetails details = TryDoAndReturn(s => s.CreateLobby(username));

            CheckLogin(Guid.Empty, details);

            lock (m_lock)
            {
                m_frontEnd.Connect(details);
            }
        }

        public void EnterLobby(Guid lobbyId, string username)
        {
            ThrowIfLoggedIn();

            LoginDetails details = TryDoAndReturn(s => s.EnterLobby(lobbyId, username));

            CheckLogin(lobbyId, details);

            lock (m_lock)
            {
                m_frontEnd.Connect(details);
            }
        }

        private static void CheckLogin(Guid lobbyId, LoginDetails details)
        {
            if (details == null)
            {
                throw new InvalidOperationException("Connection problem while logging in.");
            }

            switch (details.Result)
            {
                case LoginResult.Success:
                    break;

                case LoginResult.AlreadyLoggedIn:
                    Throw.InvalidArgumentIf(lobbyId != details.LobbyId, "Already connected to another lobby", "lobbyId");
                    break;

                case LoginResult.InvalidLobby:
                    throw new ArgumentException("Unknown lobby id");

                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #region Utils

        protected void ThrowIfLoggedIn()
        {
            Throw.InvalidOperationIf(m_frontEnd.IsConnected, "The client is already logged into another lobby.");
        }

        protected void ThrowIfNotLoggedIn()
        {
            Throw.InvalidOperationIf(!m_frontEnd.IsConnected, "Cannot access this property/method when not logged in.");
        }

        protected void AssertStateIs(ClientState state)
        {
            Throw.InvalidOperationIf(m_state == state, string.Format("Cannot do this operation while the client is in the {0} state.", m_state));
        }

        #endregion

        #region Creation Methods

        public static Client CreateLocal(LocalServer server)
        {
            return new LocalClient(server);
        }

        public static NetworkClient CreateNetwork()
        {
            return new NetworkClient();
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler Disconnected;

        protected virtual void OnDisconnected(EventArgs e)
        {
            Disconnected.Raise(this, EventArgs.Empty);
        }

        #endregion

        #region Inner Types

        protected class FrontEnd : IClientContract, ILobby, IChatService
        {
            #region Variables

            private readonly Client m_owner;
            private readonly List<User> m_users = new List<User>();
            private readonly PlayerCollection m_players = new PlayerCollection();

            private User m_user;
            private Guid m_lobbyId;

            #endregion

            #region Constructor

            public FrontEnd(Client owner)
            {
                Throw.IfNull(owner, "owner");
                m_owner = owner;
            }

            #endregion

            #region Properties

            public bool IsConnected
            {
                get { return m_user != null; }
            }

            #endregion

            #region Methods

            public void Connect(LoginDetails details)
            {
                m_user = details.User;
                m_lobbyId = details.LobbyId;

                lock (m_users)
                {
                    m_users.Add(m_user); // Server doesn't send Join/Left event for own user.
                }
            }

            public void Disconnect()
            {
                lock (m_users)
                {
                    m_users.Remove(m_user);
                }

                m_user = null;
                m_lobbyId = Guid.Empty;
            }

            #endregion

            #region Implementation

            #region Lobby

            Guid ILobby.Id
            {
                get { return m_lobbyId; }
            }

            User ILobby.User
            {
                get { return m_user; }
            }

            IChatService ILobby.Chat
            {
                get { return this; }
            }

            void IClientContract.Ping()
            {
                // Nothing to do
            }

            #region UserChanged

            void IClientContract.OnUserChanged(UserChange change, User user)
            {
                EventHandler<UserChangedEventArgs> handler;

                lock (m_users)
                {
                    switch (change)
                    {
                        case UserChange.Joined:
                            m_users.Add(user);
                            break;

                        case UserChange.Left:
                            m_users.Remove(user);
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    handler = UserChangedImpl;
                }

                handler.Raise(this, new UserChangedEventArgs(change, user));
            }

            private event EventHandler<UserChangedEventArgs> UserChangedImpl;

            event EventHandler<UserChangedEventArgs> ILobby.UserChanged
            {
                add
                {
                    lock (m_users)
                    {
                        foreach (var user in m_users)
                        {
                            value(this, new UserChangedEventArgs(UserChange.Joined, user));
                        }

                        UserChangedImpl += value;
                    }
                }
                remove
                {
                    UserChangedImpl -= value;
                }
            }

            #endregion

            #region PlayerChanged

            void IClientContract.OnPlayerChanged(PlayerChange change, Player player)
            {
                EventHandler<PlayerChangedEventArgs> handler;

                lock (m_players)
                {
                    switch (change)
                    {
                        case PlayerChange.Joined:
                            m_players.Add(player);
                            break;

                        case PlayerChange.Left:
                            m_players.Remove(player);
                            break;

                        case PlayerChange.Changed:
                            m_players.Replace(player);
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    handler = PlayerChangedImpl;
                }

                handler.Raise(this, new PlayerChangedEventArgs(change, player));
            }

            private event EventHandler<PlayerChangedEventArgs> PlayerChangedImpl;

            event EventHandler<PlayerChangedEventArgs> ILobby.PlayerChanged
            {
                add
                {
                    lock (m_players)
                    {
                        foreach (var player in m_players)
                        {
                            value(this, new PlayerChangedEventArgs(PlayerChange.Joined, player));
                        }

                        PlayerChangedImpl += value;
                    }
                }
                remove
                {
                    PlayerChangedImpl -= value;
                }
            }

            #endregion

            #endregion

            #region Chat

            void IChatService.Say(string msg)
            {
                m_owner.TryDo(s => s.Say(msg));
            }

            void IClientContract.OnMessageReceived(User user, string message)
            {
                MessageReceived.Raise(this, new MessageReceivedEventArgs(user, message));
            }

            public event EventHandler<MessageReceivedEventArgs> MessageReceived;

            #endregion

            #endregion

            #region Inner Types

            private class PlayerCollection : KeyedCollection<Guid, Player>
            {
                protected override Guid GetKeyForItem(Player item)
                {
                    return item.Id;
                }

                public void Replace(Player player)
                {
                    Remove(player.Id);
                    Add(player);
                }
            }

            #endregion
        }

        private class LocalClient : Client
        {
            #region Variables

            private readonly Server m_serverBackend;

            #endregion

            #region Constructor

            public LocalClient(Server server)
            {
                Throw.IfNull(server, "server");
                m_serverBackend = server;
            }

            #endregion

            #region Methods

            protected override IServerContract CreateServer(IClientContract client)
            {
                return ProxyGenerator<IServerContract>.CreateInterfaceProxyWithTarget(m_serverBackend, new LocalOperationInterceptor(client));
            }

            #endregion

            #region Inner Types

            private class LocalOperationInterceptor : IInterceptor
            {
                private readonly LocalOperationContext m_context;

                public LocalOperationInterceptor(IClientContract callback)
                {
                    m_context = new LocalOperationContext(callback);
                }

                public void Intercept(IInvocation invocation)
                {
                    try
                    {
                        LocalOperationContext.Current = m_context;

                        invocation.Proceed();
                    }
                    finally
                    {
                        LocalOperationContext.Current = null;
                    }
                }
            }

            #endregion
        }

        protected enum ClientState
        {
            /// <summary>
            /// New client, ready to connect
            /// </summary>
            New,
            /// <summary>
            /// Connected
            /// </summary>
            Connected,
            /// <summary>
            /// Disconnected,
            /// </summary>
            Disconnected,
            /// <summary>
            /// Faulted
            /// </summary>
            Faulted
        }

        #endregion
    }
}
