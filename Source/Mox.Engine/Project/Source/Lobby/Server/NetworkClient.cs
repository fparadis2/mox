using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Castle.Core.Interceptor;

namespace Mox.Lobby.Network
{
    public abstract class NetworkClient
    {
        #region Variables

        private readonly FrontEnd m_frontEnd;

        private IServerContract m_server;
        
        #endregion

        #region Constructor

        protected NetworkClient()
        {
            m_frontEnd = new FrontEnd(this);
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

            m_frontEnd.Disconnect();

            DeleteServer();
            m_server = null;
        }

        #endregion

        #region Login

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

        public static NetworkClient CreateLocal(ServerBackend serverBackend)
        {
            return new LocalClient(serverBackend);
        }

        #endregion

        #endregion

        #region Inner Types
        
        private class FrontEnd : IClientContract, ILobby, IChatService
        {
            #region Variables

            private readonly NetworkClient m_owner;

            private User m_user;
            private Guid m_lobbyId;

            #endregion

            #region Constructor

            public FrontEnd(NetworkClient owner)
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
            }

            public void Disconnect()
            {
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

        private class LocalClient : NetworkClient
        {
            #region Variables

            private readonly ServerBackend m_serverBackend;

            #endregion

            #region Constructor

            public LocalClient(ServerBackend server)
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
                // Nothing to do
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

    public class ActualNetworkClient : NetworkClient
    {
        #region Constants

        private const string ServiceName = "Mox";

        #endregion

        #region Variables

        private string m_host = "localhost";
        private int m_port = 3845;

        private ICommunicationObject m_serverCommunicationObject;

        #endregion

        #region Properties

        /// <summary>
        /// Host to connect to.
        /// </summary>
        /// <remarks>
        /// Can be a hostname or an ip address.
        /// </remarks>
        public string Host
        {
            get { return m_host; }
            set
            {
                ThrowIfConnected();
                m_host = value;
            }
        }

        /// <summary>
        /// The port to connect to.
        /// </summary>
        public int Port
        {
            get { return m_port; }
            set
            {
                ThrowIfConnected();
                m_port = value;
            }
        }

        #endregion

        #region Methods

        protected override IServerContract CreateServer(IClientContract client)
        {
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None, true)
            {
                ReceiveTimeout = TimeSpan.FromMinutes(2),
                SendTimeout = TimeSpan.FromMinutes(1),
                ReliableSession = { Ordered = true }
            };
            EndpointAddress address = new EndpointAddress(GetServiceAddress(ServiceName, Host, Port));
            var proxy = new ProxyServer(new InstanceContext(client), binding, address);
            m_serverCommunicationObject = proxy;

            if (m_serverCommunicationObject.State == CommunicationState.Created)
            {
                m_serverCommunicationObject.Open();
                m_serverCommunicationObject.Closed += m_serverCommunicationObject_Closed;
                m_serverCommunicationObject.Faulted += m_serverCommunicationObject_Closed;
            }

            return proxy.Server;
        }

        protected override void DeleteServer()
        {
            m_serverCommunicationObject.Closed -= m_serverCommunicationObject_Closed;
            m_serverCommunicationObject.Faulted -= m_serverCommunicationObject_Closed;

            if (m_serverCommunicationObject.State == CommunicationState.Opened)
            {
                m_serverCommunicationObject.Close();
            }

            m_serverCommunicationObject = null;
        }

        private static string GetServiceAddress(string serviceName, string hostOrIp, int port)
        {
            return string.Format("net.tcp://{0}:{1}/{2}", hostOrIp, port, serviceName);
        }

	    #endregion

        #region Event Handlers

        void m_serverCommunicationObject_Closed(object sender, EventArgs e)
        {
            // if the channel faults, notify cancellation
            /* ICommunicationObject channel = (ICommunicationObject)sender;
            if (channel.State == CommunicationState.Faulted)
            {
                concreteClient.NotifyGameCanceled();
            }
            else
            {
                concreteClient.NotifyConnectionClosed();
            }*/

            Disconnect();
        }

        #endregion

        #region Inner Types

        private class ProxyServer : DuplexClientBase<IServerContract>
        {
            #region Constructor

            public ProxyServer(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress)
                : base(callbackInstance, binding, remoteAddress)
            {
            }

            #endregion

            #region Properties

            public IServerContract Server
            {
                get { return Channel; }
            }

            #endregion
        }

        #endregion
    }
}
