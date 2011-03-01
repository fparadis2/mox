using System;
using System.Diagnostics;
using System.ServiceModel;

namespace Mox.Lobby.Network
{
    internal class NetworkServerAdapter : IServerAdapter
    {
        #region Properties

        public string SessionId
        {
            get { return Current.SessionId; }
        }

        private static OperationContext Current
        {
            get
            {
                Debug.Assert(OperationContext.Current != null);
                return OperationContext.Current;
            }
        }

        #endregion

        #region Methods

        public TCallback GetCallback<TCallback>()
        {
            return Current.GetCallbackChannel<TCallback>();
        }

        public void Disconnect(object callback)
        {
            ICommunicationObject comObject = callback as ICommunicationObject;
            if (comObject != null && comObject.State != CommunicationState.Closed)
            {
                comObject.Abort();
            }
        }

        #endregion
    }
}
