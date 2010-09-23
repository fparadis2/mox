// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Mox.Network
{
    /// <summary>
    /// Allows to connect to a Mox server.
    /// </summary>
    public class MoxClient
    {
        #region Inner Types

        private class ProxyMoxService : DuplexClientBase<IMoxService>, IMoxService
        {
            #region Constructor

            public ProxyMoxService(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress)
                : base(callbackInstance, binding, remoteAddress)
            {
            }

            #endregion

            #region IMoxService Members

            public LoginDetails Login(string userName)
            {
                return Channel.Login(userName);
            }

            public void Logout()
            {
                Channel.Logout();
            }

            #endregion
        }

        private class ProxyChatService : DuplexClientBase<IChatPrivateService>, IChatPrivateService
        {
            #region Constructor

            public ProxyChatService(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress)
                : base(callbackInstance, binding, remoteAddress)
            {
            }

            #endregion

            #region IChatService Members

            public bool Login(string serviceSessionId)
            {
                return Channel.Login(serviceSessionId);
            }

            public void Logout()
            {
                Channel.Logout();
            }

            public void Say(string message)
            {
                Channel.Say(message);
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly List<ICommunicationObject> m_clients = new List<ICommunicationObject>();
        private readonly IMoxClient m_moxClient;
        private readonly IChatClient m_chatClient;

        private IMoxService m_moxService;
        private IChatPrivateService m_chatService;

        private string m_host = "localhost";
        private int m_port = MoxHost.DefaultPort;

        private Client m_client;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public MoxClient(IMoxClient moxClient, IChatClient chatClient)
        {
            Throw.IfNull(moxClient, "moxClient");
            Throw.IfNull(chatClient, "chatClient");

            m_moxClient = moxClient;
            m_chatClient = chatClient;
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
            get;
            private set;
        }

        /// <summary>
        /// Gets the chat service to which this client is connected.
        /// </summary>
        public IChatService ChatService
        {
            get
            {
                return m_chatService;
            }
        }

        /// <summary>
        /// Gets the user associated with this client if it is connected.
        /// </summary>
        public Client ConnectedUser
        {
            get
            {
                ThrowIfNotConnected();
                return m_client;
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
            if (m_clients.Count == 0)
            {
                m_moxService = CreateProxyMoxService(m_moxClient);
                m_chatService = CreateProxyChatService(m_chatClient);
            }

            m_clients.ForEach(comObject =>
            {
                if (comObject.State == CommunicationState.Created)
                {
                    comObject.Open();
                    comObject.Closed += comObject_Faulted;
                    comObject.Faulted += comObject_Faulted;
                }
            });
        }

        private IMoxService CreateProxyMoxService(IMoxClient client)
        {
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None, true);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            binding.SendTimeout = TimeSpan.FromMinutes(5);
            binding.ReliableSession.Ordered = true;
            EndpointAddress address = new EndpointAddress(ServiceUtilities.GetServiceAddress(ServiceUtilities.Constants.MoxServiceName, Host, Port));
            return Initialize(CreateProxyMoxService(client, binding, address));
        }

        protected virtual IMoxService CreateProxyMoxService(IMoxClient implementation, Binding binding, EndpointAddress address)
        {
            return new ProxyMoxService(new InstanceContext(implementation), binding, address);
        }

        private IChatPrivateService CreateProxyChatService(IChatClient client)
        {
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            EndpointAddress address = new EndpointAddress(ServiceUtilities.GetServiceAddress(ServiceUtilities.Constants.ChatServiceName, Host, Port));
            return Initialize(CreateProxyChatService(client, binding, address));
        }

        protected virtual IChatPrivateService CreateProxyChatService(IChatClient implementation, Binding binding, EndpointAddress address)
        {
            return new ProxyChatService(new InstanceContext(implementation), binding, address);
        }

        private TService Initialize<TService>(TService service)
        {
            ICommunicationObject communicationObject = (ICommunicationObject)service;
            m_clients.Add(communicationObject);
            return service;
        }

        protected virtual string GetSessionId<TService>(TService service)
            where TService : class
        {
            return ((ClientBase<TService>)(object)service).InnerChannel.SessionId;
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

            // Try to login
            LoginDetails details = m_moxService.Login("Georges");

            m_client = details.Client;

            switch (details.Result)
            {
                case LoginResult.AlreadyLoggedIn:
                    break;

                case LoginResult.Success:
                    m_chatService.Login(GetSessionId(m_moxService));
                    break;

                default:
                    throw new NotImplementedException();
            }

            IsConnected = true;
            return IsConnected;
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

            m_client = null;
            m_clients.ForEach(comObject =>
            {
                try
                {
                    comObject.Closed -= comObject_Faulted;
                    comObject.Faulted -= comObject_Faulted;

                    if (comObject.State == CommunicationState.Opened)
                    {
                        comObject.Close();
                    }
                }
                catch { }
            });
            
            m_clients.Clear();
            IsConnected = false;
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

        void comObject_Faulted(object sender, EventArgs e)
        {
            ICommunicationObject comObject = (ICommunicationObject)sender;

            // if the channel faults, notfiy cancellation
            /*TODOif (channel.State == CommunicationState.Faulted)
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
    }
}
