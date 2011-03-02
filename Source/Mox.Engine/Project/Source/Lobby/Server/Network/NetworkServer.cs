using System;
using System.Net.Sockets;
using System.ServiceModel;
using Mox.Lobby.Network;

namespace Mox.Lobby
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false, InstanceContextMode = InstanceContextMode.Single)]
    public class NetworkServer : Server
    {
        #region Constants

        public const int DefaultPort = 6283;

        #endregion

        #region Variables

        private int m_port = DefaultPort;

        private ServiceHost m_host;

        #endregion

        #region Constructor

        internal NetworkServer()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// The port on which the server is to run.
        /// </summary>
        public int Port
        {
            get { return m_port; }
            set
            {
                if (m_port != value)
                {
                    Throw.InvalidOperationIf(IsStarted, "Cannot change the port when the host is open.");
                    m_port = value;
                }
            }
        }

        protected override string CurrentSessionId
        {
            get { return OperationContext.Current.SessionId; }
        }

        #endregion

        #region Methods

        protected override bool StartImpl()
        {
            string hostName;
            try
            {
                hostName = System.Net.Dns.GetHostName();
            }
            catch (SocketException e)
            {
#warning TODO: System logging
                //Log(new LogMessage() { Importance = LogImportance.Error, Text = string.Format("Could not get local host name: {0}", e.Message) });
                return false;
            }

            //Log(new LogMessage() { Importance = LogImportance.Low, Text = string.Format("Initializing server on {0}:{1}", hostName, Port) });

            m_host = CreateHost(hostName);

            try
            {
                m_host.Open();
                m_host.Faulted += m_host_Faulted;
            }
            catch
            {
                // Log
                return false;
            }

            return true;
        }

        protected override void StopImpl()
        {
            try
            {
                m_host.Close(TimeSpan.FromMilliseconds(10));
            }
            catch
            {
                try
                {
                    m_host.Abort();
                }
                catch { }
            }

            m_host.Faulted -= m_host_Faulted;
            m_host = null;

            base.StopImpl();
        }

        protected override IClientContract GetCurrentCallback()
        {
            return OperationContext.Current.GetCallbackChannel<IClientContract>();
        }

        protected override void Disconnect(IClientContract callback)
        {
            ICommunicationObject comObject = callback as ICommunicationObject;
            if (comObject != null && comObject.State != CommunicationState.Closed)
            {
                comObject.Abort();
            }

            base.Disconnect(callback);
        }

        private ServiceHost CreateHost(string hostName)
        {
            var host = new ServiceHost(this);

            NetTcpBinding netTcpBinding = new NetTcpBinding(SecurityMode.None, true)
            {
                ReliableSession = { Ordered = true }, 
                ReceiveTimeout = TimeSpan.FromMinutes(2), 
                SendTimeout = TimeSpan.FromSeconds(2)
            };

            host.AddServiceEndpoint(typeof(IServerContract), netTcpBinding, GetServiceAddress(hostName, Port));

            return host;
        }

        internal static string GetServiceAddress(string hostOrIp, int port)
        {
            return string.Format("net.tcp://{0}:{1}/Mox", hostOrIp, port);
        }

        #endregion

        #region Event Handlers
        
        void m_host_Faulted(object sender, EventArgs e)
        {
            // TODO: Log message
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
#endif
        }

        #endregion
    }
}