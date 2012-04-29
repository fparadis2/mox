using System;
using System.Collections.Generic;

namespace Mox.Lobby
{
    internal class PendingRequests
    {
        #region Variables
        
        private readonly object m_lock = new object();
        private readonly Dictionary<int, AsyncResult> m_pendingResults = new Dictionary<int, AsyncResult>();
        private int m_nextId = 1;

        #endregion

        #region Methods

        public int Add(AsyncResult result)
        {
            int resultId;

            lock (m_lock)
            {
                resultId = AllocateNextId();
                m_pendingResults.Add(resultId, result);
            }

            return resultId;
        }

        public bool Consider(Message message, Action<System.Action> actionDispatcher)
        {
            AsyncResult asyncResult;

            lock (m_lock)
            {
                var requestId = message.RequestId;

                if (m_pendingResults.TryGetValue(requestId, out asyncResult))
                {
                    m_pendingResults.Remove(requestId);
                }
            }

            if (asyncResult != null)
            {
                actionDispatcher(() => asyncResult.Complete(message));
                return true;
            }

            return false;
        }

        private int AllocateNextId()
        {
            int nextId;
            do
            {
                nextId = unchecked(m_nextId++);
            }
            while (nextId == 0);
            return nextId;
        }

        #endregion
    }
}