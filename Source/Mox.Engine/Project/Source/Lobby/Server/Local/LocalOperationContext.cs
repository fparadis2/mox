using System;

namespace Mox.Lobby.Network
{
    internal class LocalOperationContext : IServerAdapter
    {
        #region Variables

        [ThreadStatic]
        private static LocalOperationContext ms_currentContext;

        private readonly string m_sessionId;
        private readonly object m_callback;

        #endregion

        #region Constructor

        public LocalOperationContext(object callback)
        {
            Throw.IfNull(callback, "callback");
            m_callback = callback;
            m_sessionId = Guid.NewGuid().ToString();
        }

        #endregion

        #region Static Instance

        public static LocalOperationContext Current
        {
            get { return ms_currentContext; }
            set { ms_currentContext = value; }
        }

        #endregion

        #region Properties

        public string SessionId
        {
            get { return m_sessionId; }
        }

        #endregion

        #region Methods

        public TCallback GetCallback<TCallback>()
        {
            return (TCallback)m_callback;
        }

        public void Disconnect(object callback)
        {
            // Nothing to do
        }

        #endregion
    }
}
