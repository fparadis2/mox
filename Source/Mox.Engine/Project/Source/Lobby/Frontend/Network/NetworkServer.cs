using System;
using System.Net;
using System.Net.Sockets;
using Mox.Threading;

namespace Mox.Lobby
{
    public class NetworkServer : Server
    {
        #region Constants

        public const int DefaultPort = 6283;

        #endregion

        #region Variables

        private readonly MessageQueue m_sendQueue = new MessageQueue(WakeUpJob.FromThreadPool());

        private int m_port = DefaultPort;

        private TcpListener m_listener;

        #endregion

        #region Constructor

        internal NetworkServer(ILog log)
            : base(log)
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

        #endregion

        #region Methods

        protected override bool StartImpl()
        {
            string hostName;
            try
            {
                hostName = Dns.GetHostName();
            }
            catch (SocketException e)
            {
                Log.LogError("Could not get local host name: {0}", e.Message);
                return false;
            }

            Log.Log(LogImportance.Low, "Initializing server on {0}:{1}", hostName, Port);

            m_listener = new TcpListener(IPAddress.Any, Port);
            m_listener.Start();

            m_listener.BeginAcceptTcpClient(OnAcceptTcpClient, m_listener);

            return true;
        }

        protected override void StopImpl()
        {
            m_listener.Stop();
            m_listener = null;

            base.StopImpl();
        }

        private void OnAcceptTcpClient(System.IAsyncResult result)
        {
            TcpListener listener = (TcpListener)result.AsyncState;
            TcpClient tcpClient;

            try
            {
                tcpClient = listener.EndAcceptTcpClient(result);
            }
            catch (ObjectDisposedException)
            {
                // Ignore (server is closing)
                return;
            }

            OnClientConnected(new ServerTcpChannel(tcpClient, m_sendQueue));
            listener.BeginAcceptTcpClient(OnAcceptTcpClient, listener);
        }

        #endregion

        #region Inner Types

        private class ServerTcpChannel : TcpChannel
        {
            public ServerTcpChannel(TcpClient client, MessageQueue sendQueue)
                : base(client, sendQueue)
            {
            }
        }

        #endregion
    }
}