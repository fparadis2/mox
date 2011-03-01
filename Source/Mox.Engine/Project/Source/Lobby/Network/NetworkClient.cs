using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Mox.Lobby.Network
{
    public class NetworkClient
    {
        #region Constants

        private const string ServiceName = "Mox";

        #endregion

        #region Variables

        private readonly FrontEnd m_frontEnd;

        private IServerContract m_server;
        private ICommunicationObject m_serverCommunicationObject;

#warning TODO: Move in adapter
        private string m_host = "localhost";
        private int m_port = 3845;

        #endregion

        #region Constructor

        public NetworkClient()
        {
            m_frontEnd = new FrontEnd(this);
        }

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
                ThrowIfNotConnected();
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
                CreateServer();

                if (m_serverCommunicationObject.State == CommunicationState.Created)
                {
                    m_serverCommunicationObject.Open();
                    m_serverCommunicationObject.Closed += m_serverCommunicationObject_Closed;
                    m_serverCommunicationObject.Faulted += m_serverCommunicationObject_Closed;
                }
            }
        }

        private void CreateServer()
        {
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None, true)
            {
                ReceiveTimeout = TimeSpan.FromMinutes(2),
                SendTimeout = TimeSpan.FromMinutes(1),
                ReliableSession = { Ordered = true }
            };
            EndpointAddress address = new EndpointAddress(GetServiceAddress(ServiceName, Host, Port));
            var proxy = new ProxyServer(new InstanceContext(m_frontEnd), binding, address);
            m_server = proxy.Server;
            m_serverCommunicationObject = proxy;
        }

        private static string GetServiceAddress(string serviceName, string hostOrIp, int port)
        {
            return string.Format("net.tcp://{0}:{1}/{2}", hostOrIp, port, serviceName);
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
            if (IsConnected)
            {
                return true;
            }

            Initialize();

#warning TODO: Join existing lobby & use actual name
            // Try to login
            LoginDetails details = m_server.CreateLobby("Georges");

            m_frontEnd.Connect(details);

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

            m_serverCommunicationObject.Closed -= m_serverCommunicationObject_Closed;
            m_serverCommunicationObject.Faulted -= m_serverCommunicationObject_Closed;

            if (m_serverCommunicationObject.State == CommunicationState.Opened)
            {
                m_serverCommunicationObject.Close();
            }
            
            m_serverCommunicationObject = null;
            m_server = null;
        }

        #endregion

        #region Utils

        private void ThrowIfConnected()
        {
            Throw.InvalidOperationIf(IsConnected, "Cannot change the state of the client while it is connected.");
        }

        private void ThrowIfNotConnected()
        {
            Throw.InvalidOperationIf(!IsConnected, "Cannot access this property/method when not connected.");
        }

        #endregion

        #endregion

        #region Event Handlers

        void m_serverCommunicationObject_Closed(object sender, EventArgs e)
        {
            // if the channel faults, notfiy cancellation
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

        #endregion
    }
}
