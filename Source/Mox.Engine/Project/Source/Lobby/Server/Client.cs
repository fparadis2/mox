using System;
using System.Collections.Generic;
using Castle.Core.Interceptor;

using Mox.Lobby.Network;

namespace Mox.Lobby
{
    public abstract class Client
    {
        #region Variables

        private readonly FrontEnd m_frontEnd;

        private IServerContract m_server;
        
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
            get { return m_server != null; }
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

        private IServerContract Server
        {
            get
            {
                ThrowIfNotConnected();
                return m_server;
            }
        }

        #endregion

        #region Methods

        #region Initialization

        /// <summary>
        /// Must be called once before connecting.
        /// </summary>
        private void Initialize()
        {
            if (m_server == null)
            {
                m_server = CreateServer(m_frontEnd);
            }
        }

        protected abstract IServerContract CreateServer(IClientContract client);
        protected abstract void DeleteServer();

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
            if (IsConnected)
            {
                return true;
            }

            Initialize();
            return true;
        }

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        public void Disconnect()
        {
            if (!IsConnected)
            {
                return;
            }

            m_server.Logout();
            DeleteServer();
            m_server = null;
        }

        protected void DisconnectImpl()
        {
            m_frontEnd.Disconnect();
        }

        #endregion

        #region Login

        public IEnumerable<Guid> GetLobbies()
        {
            ThrowIfNotConnected();

            return m_server.GetLobbies();
        }

        public void CreateLobby(string username)
        {
            ThrowIfLoggedIn();

            LoginDetails details = m_server.CreateLobby(username);

            CheckLogin(Guid.Empty, details);

            m_frontEnd.Connect(details);
        }

        public void EnterLobby(Guid lobbyId, string username)
        {
            ThrowIfLoggedIn();

            LoginDetails details = m_server.EnterLobby(lobbyId, username);

            CheckLogin(lobbyId, details);

            m_frontEnd.Connect(details);
        }

        private static void CheckLogin(Guid lobbyId, LoginDetails details)
        {
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

        protected void ThrowIfConnected()
        {
            Throw.InvalidOperationIf(IsConnected, "Cannot change the state of the client while it is connected.");
        }

        protected void ThrowIfNotConnected()
        {
            Throw.InvalidOperationIf(!IsConnected, "Cannot access this property/method when not connected.");
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

        #region Inner Types

        protected class FrontEnd : IClientContract, ILobby, IChatService
        {
            #region Variables

            private readonly Client m_owner;
            private readonly List<User> m_users = new List<User>();

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

            private IServerContract Server
            {
                get { return m_owner.Server; }
            }

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

            #region Chat

            void IChatService.Say(string msg)
            {
                Server.Say(msg);
            }

            void IClientContract.OnMessageReceived(User user, string message)
            {
                MessageReceived.Raise(this, new MessageReceivedEventArgs(user, message));
            }

            public event EventHandler<MessageReceivedEventArgs> MessageReceived;

            #endregion

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

            protected override void DeleteServer()
            {
                DisconnectImpl();
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

        #endregion
    }
}
