﻿using System;

namespace Mox.Lobby.Network
{
    public class LocalServerAdapter : IServerAdapter
    {
        #region Properties

        public string SessionId
        {
            get { return CurrentContext.SessionId; }
        }

        private LocalOperationContext CurrentContext
        {
            get
            {
                var currentContext = LocalOperationContext.Current;
                Throw.InvalidOperationIf(currentContext == null, "No current local operation context. Are you calling the server backend directly? You need to use a local server connection.");
                return currentContext;
            }
        }

        #endregion

        #region Methods

        public TCallback GetCallback<TCallback>()
        {
            return CurrentContext.GetCallback<TCallback>();
        }

        #endregion
    }
}
