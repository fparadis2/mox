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
using System.Net.Sockets;

namespace Mox.Network
{
    /// <summary>
    /// Hosts all the services offered by Mox.
    /// </summary>
    public class MoxHost : IServiceManager
    {
        #region Inner Types

        /// <summary>
        /// Used for tests.
        /// </summary>
        public interface IServiceHost
        {
            void AddServiceEndpoint(Type type, NetTcpBinding netTcpBinding, string endpointAddress);
            void Open();
            void Close(TimeSpan timeout);
            void Abort();

            event EventHandler Faulted;            
        }

        private class DefaultServiceHost : IServiceHost
        {
            #region Variables

            private readonly ServiceHost m_serviceHost;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="singletonInstance"></param>
            public DefaultServiceHost(object singletonInstance)
            {
                Throw.IfNull(singletonInstance, "singletonInstance");
                m_serviceHost = new ServiceHost(singletonInstance);
            }

            #endregion

            #region IServiceHost Members

            public void AddServiceEndpoint(Type type, NetTcpBinding netTcpBinding, string endpointAddress)
            {
                m_serviceHost.AddServiceEndpoint(type, netTcpBinding, endpointAddress);
            }

            public void Open()
            {
                m_serviceHost.Open();
            }

            public void Close(TimeSpan timeout)
            {
                m_serviceHost.Close(timeout);
            }

            public void Abort()
            {
                m_serviceHost.Abort();
            }

            public event EventHandler Faulted
            {
                add { m_serviceHost.Faulted += value; }
                remove { m_serviceHost.Faulted -= value; }
            }

            #endregion
        }

        private class DefaultOperationContext : IOperationContext
        {
            #region IOperationContext Members

            public string SessionId
            {
                get { return System.ServiceModel.OperationContext.Current.SessionId; }
            }

            public T GetCallbackChannel<T>()
            {
                return System.ServiceModel.OperationContext.Current.GetCallbackChannel<T>();
            }

            public string LocalHostDns
            {
                get { return System.Net.Dns.GetHostName(); }
            }

            #endregion
        }

        #endregion

        #region Constants

        public const int DefaultPort = 6789; // TODO: find a suitable port.

        #endregion

        #region Variables

        private readonly IOperationContext m_operationContext;
        private readonly IMainService m_mainService;
        private readonly ChatService m_chatService;

        private readonly List<IServiceHost> m_hosts = new List<IServiceHost>();
        private readonly List<ILog> m_logs = new List<ILog>();

        private int m_port = DefaultPort;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public MoxHost()
            : this(new DefaultOperationContext())
        {
        }

        private MoxHost(IOperationContext operationContext)
            : this(operationContext, new MainService(operationContext))
        {
        }

        /// <summary>
        /// Full Constructor (for tests).
        /// </summary>
        internal MoxHost(IOperationContext operationContext, IMainService mainService)
        {
            Throw.IfNull(operationContext, "operationContext");
            Throw.IfNull(mainService, "mainService");

            m_operationContext = operationContext;
            m_mainService = mainService;

            m_chatService = new ChatService(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Logs.
        /// </summary>
        public ICollection<ILog> Logs
        {
            get { return m_logs; }
        }

        /// <summary>
        /// Chat Service.
        /// </summary>
        public ChatService ChatService
        {
            get { return m_chatService; }
        }
                
        /// <summary>
        /// Returns true if the host is open.
        /// </summary>
        public bool IsOpen
        {
            get { return m_hosts.Count > 0; }
        }

        /// <summary>
        /// The port on which the host is to run.
        /// </summary>
        public int Port
        {
            get { return m_port; }
            set 
            {
                if (m_port != value)
                {
                    Throw.InvalidOperationIf(IsOpen, "Cannot change the port when the host is open.");
                    m_port = value;
                }
            }
        }

        #endregion

        #region Methods

        #region Open

        /// <summary>
        /// Opens the host.
        /// </summary>
        /// <remarks>
        /// Returns true if the host was opened correctly.
        /// </remarks>
        public bool Open()
        {
            Throw.InvalidOperationIf(IsOpen, "Host is already open");

            string hostName;
            try
            {
                hostName = OperationContext.LocalHostDns;
            }
            catch (SocketException e)
            {
                Log(new LogMessage() { Importance = LogImportance.Error, Text = string.Format("Could not get local host name: {0}", e.Message) });
                return false;
            }

            Log(new LogMessage() { Importance = LogImportance.Low, Text = string.Format("Initializing server on {0}:{1}", hostName, Port) });

            m_hosts.Add(CreateMainHost(hostName));
            m_hosts.Add(CreateChatHost(hostName));

            m_hosts.ForEach(host =>
            {
                host.Faulted += host_Faulted;
                host.Open();
            });

            Log(new LogMessage() { Importance = LogImportance.Low, Text = "Server is running..." });

            return true;
        }

        private IServiceHost CreateMainHost(string hostName)
        {
            var host = CreateServiceHost(m_mainService);

            NetTcpBinding netTcpBinding = new NetTcpBinding(SecurityMode.None, true);
            netTcpBinding.ReliableSession.Ordered = true;
            netTcpBinding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            netTcpBinding.SendTimeout = TimeSpan.FromSeconds(15);

            host.AddServiceEndpoint(typeof(IMoxService), netTcpBinding, ServiceUtilities.GetServiceAddress(ServiceUtilities.Constants.MoxServiceName, hostName, Port));

            return host;
        }

        private IServiceHost CreateChatHost(string hostName)
        {
            var host = CreateServiceHost(m_chatService);

            NetTcpBinding netTcpBinding = new NetTcpBinding(SecurityMode.None);
            netTcpBinding.SendTimeout = TimeSpan.FromSeconds(15);

            host.AddServiceEndpoint(typeof(IChatPrivateService), netTcpBinding, ServiceUtilities.GetServiceAddress(ServiceUtilities.Constants.ChatServiceName, hostName, Port));

            return host;
        }

        private void host_Faulted(object sender, EventArgs e)
        {
            // TODO: Log message
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
#endif
        }

        /// <summary>
        /// Creates a service host for the given singleton instance.
        /// </summary>
        /// <remarks>
        /// Virtual for tests.
        /// </remarks>
        /// <param name="singletonInstance"></param>
        /// <returns></returns>
        protected virtual IServiceHost CreateServiceHost(object singletonInstance)
        {
            return new DefaultServiceHost(singletonInstance);
        }

        #endregion

        #region Close

        /// <summary>
        /// Closes the host.
        /// </summary>
        public void Close()
        {
            m_hosts.ForEach(host =>
            {
                try
                {
                    host.Close(TimeSpan.FromSeconds(2));
                }
                catch (TimeoutException)
                {
                    host.Abort();
                }
            });

            m_hosts.Clear();
        }

        #endregion

        #region Logging

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message"></param>
        public void Log(LogMessage message)
        {
            Logs.ForEach(log => log.Log(message));
        }

        #endregion

        #endregion

        #region IServiceManager Members

        /// <summary>
        /// Operation Context.
        /// </summary>
        public IOperationContext OperationContext
        {
            get { return m_operationContext; }
        }

        /// <summary>
        /// Main Service.
        /// </summary>
        public IMainService MainService
        {
            get { return m_mainService; }
        }

        #endregion
    }
}
