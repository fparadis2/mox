using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Mox.Lobby2
{
    internal class PendingRequest
    {
        public virtual bool Consider(Message message)
        {
            return false;
        }
    }

    internal class PendingRequest<TMessageType> : PendingRequest
                where TMessageType : Message
    {
        private readonly ManualResetEvent m_event = new ManualResetEvent(false);
        private TMessageType m_result;

        public override bool Consider(Message message)
        {
            if (m_result == null && message is TMessageType)
            {
                m_result = (TMessageType)message;
                m_event.Set();
                return true;
            }

            return false;
        }

        public TMessageType Consume()
        {
            m_event.WaitOne();
            m_event.Close();

            Debug.Assert(m_result != null);
            return m_result;
        }
    }
}
