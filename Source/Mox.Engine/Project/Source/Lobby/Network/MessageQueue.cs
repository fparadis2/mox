using System;
using System.Collections.Generic;
using System.Threading;
using Mox.Threading;

namespace Mox.Lobby
{
    internal class MessageQueue
    {
        #region Variables

        private readonly WakeUpJob m_job;
        private readonly object m_lock = new object();
        private List<PendingMessage> m_messages = new List<PendingMessage>();

        #endregion

        #region Constructor

        public MessageQueue(WakeUpJob job)
        {
            m_job = job;
            m_job.Action = ProcessMessages;
        }

        #endregion

        #region Methods

        public void Join()
        {
            m_job.Join();
        }

        public void Enqueue(Message message, Action<Message> messageAction)
        {
            PendingMessage pending = new PendingMessage(message, messageAction);

            lock (m_lock)
            {
                m_messages.Add(pending);
            }

            m_job.WakeUp();
        }

        public void ProcessMessages()
        {
            List<PendingMessage> messagesToProcess;

            lock (m_lock)
            {
                messagesToProcess = m_messages;
                m_messages = new List<PendingMessage>();
            }

            try
            {
                foreach (var message in messagesToProcess)
                {
                    message.Process();
                }
            }
            catch {}
        }

        #endregion

        #region Inner Types

        private class PendingMessage
        {
            private readonly Message m_message;
            private readonly Action<Message> m_messageAction;

            public PendingMessage(Message message, Action<Message> messageAction)
            {
                m_message = message;
                m_messageAction = messageAction;
            }

            public void Process()
            {
                m_messageAction(m_message);
            }
        }

        #endregion
    }
}
