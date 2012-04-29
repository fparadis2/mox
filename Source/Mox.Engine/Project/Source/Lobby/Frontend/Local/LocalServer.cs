using System;

namespace Mox.Lobby
{
    public class LocalServer : Server
    {
        #region Constructor

        internal LocalServer(ILog log)
            : base(log)
        {
        }

        #endregion

        #region Methods

        internal void CreateConnection(LocalChannel channel)
        {
            OnClientConnected(channel);
        }

        #endregion
    }
}