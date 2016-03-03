using System;
using System.Threading.Tasks;

namespace Mox.Lobby
{
    public interface IChannel
    {
        Task<TResponse> Request<TRequest, TResponse>(TRequest message)
            where TRequest : Request<TResponse>
            where TResponse : Response;

        void Respond<TRequest, TResponse>(TRequest request, TResponse response)
            where TRequest : Request<TResponse>
            where TResponse : Response;

        void Send(Message message);

        event EventHandler<MessageReceivedEventArgs> MessageReceived;

#warning [Medium] Handle disconnection in client
        event EventHandler Disconnected;
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        private readonly Message m_message;

        public MessageReceivedEventArgs(Message message)
        {
            m_message = message;
        }

        public Message Message
        {
            get { return m_message; }
        }
    }
}
