using System;
using Mox.Lobby.Network;

namespace Mox.Lobby.Server
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