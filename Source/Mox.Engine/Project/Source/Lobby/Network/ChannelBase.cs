using System;

namespace Mox.Lobby
{
    internal abstract class ChannelBase : IChannel
    {
        #region Variables

        private readonly PendingRequests m_pendingRequests = new PendingRequests();

        #endregion

        #region Implementation of IChannel

        public IAsyncResult<TResponse> BeginRequest<TResponse>(Message message) where TResponse : Message
        {
            AsyncResult<TResponse> result = new AsyncResult<TResponse>();
            message.RequestId = m_pendingRequests.Add(result);

            Send(message);

            return result;
        }

        public virtual TResponse Request<TResponse>(Message message) where TResponse : Message
        {
            var result = BeginRequest<TResponse>(message);
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

        protected virtual void ExecuteOnRead(System.Action action)
        {
            action();
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
            Disconnected.Raise(this, EventArgs.Empty);
        }

        #endregion
    }
}