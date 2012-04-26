using System;

namespace Mox.Lobby
{
    public class NetworkServer : Server
    {
        #region Constants

        public const int DefaultPort = 6283;

        #endregion

        #region Variables

        private int m_port = DefaultPort;

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
            //string hostName;
            //try
            //{
            //    hostName = System.Net.Dns.GetHostName();
            //}
            //catch (SocketException e)
            //{
            //    Log.LogError("Could not get local host name: {0}", e.Message);
            //    return false;
            //}

            //Log.Log(LogImportance.Low, "Initializing server on {0}:{1}", hostName, Port);

            //m_host = CreateHost(hostName);

            //try
            //{
            //    m_host.Open();
            //    m_host.Faulted += m_host_Faulted;
            //}
            //catch
            //{
            //    // Log
            //    return false;
            //}

            //var pingThread = new Thread(PingAllClients);
            //pingThread.IsBackground = true;
            //pingThread.Start();

            return true;
        }

        protected override void StopImpl()
        {
            //try
            //{
            //    m_host.Close(TimeSpan.FromMilliseconds(10));
            //}
            //catch
            //{
            //    try
            //    {
            //        m_host.Abort();
            //    }
            //    catch { }
            //}

            //m_host.Faulted -= m_host_Faulted;
            //m_host = null;

            base.StopImpl();
        }

        #endregion
    }
}