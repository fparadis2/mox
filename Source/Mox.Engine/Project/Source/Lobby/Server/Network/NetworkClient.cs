using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Mox.Lobby.Network
{
    public class NetworkClient : Client
    {
        #region Variables

        private string m_host = "localhost";
        private int m_port = NetworkServer.DefaultPort;

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
                AssertStateIs(ClientState.New);
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
                AssertStateIs(ClientState.New);
                m_port = value;
            }
        }

        #endregion

        #region Methods

        protected override FrontEnd CreateFrontEnd(Client client)
        {
            return new NetworkFrontEnd(client);
        }

        protected override IServerContract CreateServer(IClientContract client)
        {
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None, true)
            {
                ReliableSession = { Ordered = true }
            };
            EndpointAddress address = new EndpointAddress(NetworkServer.GetServiceAddress(Host, Port));
            var proxy = new ProxyServer(new InstanceContext(client), binding, address);
            m_serverCommunicationObject = proxy;

            try
            {
                if (m_serverCommunicationObject.State == CommunicationState.Created)
                {
                    m_serverCommunicationObject.Open();

                    m_serverCommunicationObject.Closed += m_serverCommunicationObject_Closed;
                    m_serverCommunicationObject.Faulted += m_serverCommunicationObject_Closed;
                }
            }
            catch
            {
                DeleteServer();
                return null;
            }

            return proxy.Server;
        }

        protected override void DeleteServerImpl()
        {
            base.DeleteServerImpl();

            m_serverCommunicationObject.Closed -= m_serverCommunicationObject_Closed;
            m_serverCommunicationObject.Faulted -= m_serverCommunicationObject_Closed;

            if (m_serverCommunicationObject.State == CommunicationState.Opened)
            {
                m_serverCommunicationObject.Close();
            }

            m_serverCommunicationObject = null;
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

            DeleteServer();
        }

        #endregion

        #region Inner Types

        [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
        private class NetworkFrontEnd : FrontEnd
        {
            public NetworkFrontEnd(Client owner)
                : base(owner)
            {
            }
        }
        
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