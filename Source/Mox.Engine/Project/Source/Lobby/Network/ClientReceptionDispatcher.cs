using Mox.Threading;

namespace Mox.Lobby
{
    internal class ClientReceptionDispatcher : IReceptionDispatcher
    {
        #region Variables

        private readonly IDispatcher m_dispatcher;
        private readonly MessageQueue m_receiveQueue;

        #endregion

        #region Constructor

        public ClientReceptionDispatcher(IDispatcher dispatcher)
        {
            m_dispatcher = dispatcher;
            m_receiveQueue = new MessageQueue(WakeUpJob.FromDispatcher(dispatcher));
        }

        #endregion

        #region Implementation of IReceptionDispatcher

        public bool ReceiveMessagesSynchronously
        {
            get { return true; }
        }

        public void BeginInvoke(System.Action action)
        {
            m_receiveQueue.Enqueue(action);
        }

        public void Invoke(System.Action action)
        {
            m_dispatcher.InvokeIfNeeded(action);
        }

        public void OnAfterRequest()
        {
            // This works because receive queue is bound to main thread so we can safely call this (Request is always made from main thread on client).
            m_receiveQueue.ProcessMessages();
        }

        #endregion
    }
}