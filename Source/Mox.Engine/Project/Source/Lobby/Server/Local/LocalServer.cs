using System;
using Mox.Lobby.Network;

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

        #region Properties

        protected override string CurrentSessionId
        {
            get { return CurrentContext.SessionId; }
        }

        private static LocalOperationContext CurrentContext
        {
            get
            {
                var currentContext = LocalOperationContext.Current;
                Throw.InvalidOperationIf(currentContext == null, "No current local operation context. Are you calling the server backend directly? You need to create a client using Client.CreateLocal");
                return currentContext;
            }
        }

        #endregion

        #region Methods

        protected override IClientContract GetCurrentCallback()
        {
            return CurrentContext.GetCallback<IClientContract>();
        }

        #endregion
    }
}