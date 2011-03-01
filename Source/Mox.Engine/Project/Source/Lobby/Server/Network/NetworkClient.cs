using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Mox.Lobby.Network
{
    public class NetworkClient : Client
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