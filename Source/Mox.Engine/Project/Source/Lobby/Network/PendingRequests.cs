using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Mox.Lobby
{
    internal interface PendingRequest
    {
        void Complete(Response result);
        void Fail();
    }

    internal class TaskPendingRequest<T> : PendingRequest
        where T : Response
    {
        private readonly TaskCompletionSource<T> m_completionSource = new TaskCompletionSource<T>();
        
        public Task<T> Task
        {
            get { return m_completionSource.Task; }
        }

        public void Complete(Response result)
        {
            Debug.Assert(result != null);

            T response = result as T;
            if (response == null)
            {
                string exceptionMessage = string.Format("Expected response of type {0} but got {1}", typeof(T).Name, result.GetType().Name);
                m_completionSource.SetException(new InvalidOperationException(exceptionMessage));
            }
            else
            {
                m_completionSource.SetResult((T)result);
            }
        }

        public void Fail()
        {
            m_completionSource.SetCanceled();
        }
    }

    internal class PendingRequests
    {
        #region Variables
        
        private readonly object m_lock = new object();
        private readonly Dictionary<ushort, PendingRequest> m_pendingResults = new Dictionary<ushort, PendingRequest>();
        private ushort m_nextId = 1;

        #endregion

        #region Methods

        public ushort Add(PendingRequest request)
        {
            ushort resultId;

            lock (m_lock)
            {
                resultId = AllocateNextId();
                m_pendingResults.Add(resultId, request);
            }

            return resultId;
        }

        public bool Consider(Response response)
        {
            PendingRequest request;

            lock (m_lock)
            {
                var requestId = response.RequestId;

                if (m_pendingResults.TryGetValue(requestId, out request))
                {
                    m_pendingResults.Remove(requestId);
                }
            }

            if (request != null)
            {
                request.Complete(response);
                return true;
            }

            return false;
        }

        public void FailAll()
        {
            lock (m_lock)
            {
                foreach (var request in m_pendingResults.Values)
                {
                    request.Fail();
                }
                m_pendingResults.Clear();
            }
        }

        private ushort AllocateNextId()
        {
            ushort nextId;
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