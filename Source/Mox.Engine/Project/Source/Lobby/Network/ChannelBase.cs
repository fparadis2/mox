using System;

namespace Mox.Lobby
{
    internal abstract class ChannelBase : IChannel
    {
        #region Variables

        private readonly PendingRequests m_pendingRequests = new PendingRequests();
        private IReceptionDispatcher m_receptionDispatcher = new FreeReceptionDispatcher();

        #endregion

        #region Properties

        public IReceptionDispatcher ReceptionDispatcher
        {
            get { return m_receptionDispatcher; }
            set { m_receptionDispatcher = value ?? new FreeReceptionDispatcher(); }
        }

        #endregion

        #region Implementation of IChannel

        public IAsyncResult<TResponse> BeginRequest<TResponse>(Message message) where TResponse : Message
        {
            AsyncResult<TResponse> result = new AsyncResult<TResponse>();
            message.RequestId = m_pendingRequests.Add(result);

            Send(message);

            return result;
        }

        public TResponse Request<TResponse>(Message message) where TResponse : Message
        {
            var result = BeginRequest<TResponse>(message);
            ReceptionDispatcher.OnAfterRequest();
            return result.Value;
        }

        public abstract void Send(Message message);

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        protected void OnMessageReceived(Message message)
        {
            if (!m_pendingRequests.Consider(message, ExecuteOnRead))
            {
                ExecuteOnRead(() => RaiseMessageReceived(message));
            }
        }

        private void ExecuteOnRead(System.Action action)
        {
            ReceptionDispatcher.BeginInvoke(action);
        }

        private void RaiseMessageReceived(Message message)
        {
            MessageReceived.Raise(this, new MessageReceivedEventArgs(message));
        }

        #endregion

        #region Events

        public event EventHandler Disconnected;

        protected virtual void OnDisconnected()
        {
            m_pendingRequests.FailAll();

            ReceptionDispatcher.Invoke(() => Disconnected.Raise(this, EventArgs.Empty));
        }

        #endregion
    }
}