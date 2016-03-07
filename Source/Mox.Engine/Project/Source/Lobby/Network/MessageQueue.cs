using System;
using System.Collections.Generic;
using Mox.Threading;

namespace Mox.Lobby.Network
{
    internal class MessageQueue
    {
        #region Variables

        private readonly WakeUpJob m_job;
        private readonly object m_lock = new object();
        private List<System.Action> m_messages = new List<System.Action>();

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

        public void Enqueue(System.Action action)
        {
            lock (m_lock)
            {
                m_messages.Add(action);
            }

            m_job.WakeUp();
        }

        public void ProcessMessages()
        {
            List<System.Action> messagesToProcess;

            lock (m_lock)
            {
                messagesToProcess = m_messages;
                m_messages = new List<System.Action>();
            }


            foreach (var message in messagesToProcess)
            {
                try
                {
                    message();
                }
#warning [LOW] Log exceptions?
                catch { }
            }
        }

        #endregion
    }
}
