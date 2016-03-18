using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Mox.Lobby.Network
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

        public abstract string EndPointIdentifier { get; }

        #endregion

        #region Implementation of IChannel

        public Task<TResponse> Request<TRequest, TResponse>(TRequest message) 
            where TRequest : Request<TResponse> 
            where TResponse : Response
        {
            TaskPendingRequest<TResponse> request = new TaskPendingRequest<TResponse>();
            message.RequestId = m_pendingRequests.Add(request);
            Send(message);
            return request.Task;
        }

        public void Respond<TRequest, TResponse>(TRequest request, TResponse response) 
            where TRequest : Request<TResponse> 
            where TResponse : Response
        {
            Respond((Request)request, (Response)response);
        }

        private void Respond(Request request, Response response)
        {
            response.RequestId = request.RequestId;
            Send(response);
        }

        public void Send(Message message)
        {
            ValidateSentMessage(message);
            SendMessage(message);
        }

        [Conditional("DEBUG")]
        private void ValidateSentMessage(Message message)
        {
            var request = message as Request;
            if (request != null)
            {
                Throw.InvalidOperationIf(request.RequestId == 0, "Request should not have an invalid request id when being sent");
            }

            var response = message as Response;
            if (response != null)
            {
                Throw.InvalidOperationIf(response.RequestId == 0, "Response should not have an invalid request id when being sent back");
            }
        }

        protected abstract void SendMessage(Message message);

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        protected void OnMessageReceived(Message message)
        {
            Response response = message as Response;
            if (response != null)
            {
                m_pendingRequests.Consider(response);
                return;
            }

            ExecuteOnRead(() => RaiseMessageReceived(message));
        }

        private void ExecuteOnRead(System.Action action)
        {
            ReceptionDispatcher.BeginInvoke(action);
        }

        private void RaiseMessageReceived(Message message)
        {
            var e = new MessageReceivedEventArgs(message);
            MessageReceived.Raise(this, e);

            if (e.Response != null)
            {
                Respond((Request)message, e.Response);
            }
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