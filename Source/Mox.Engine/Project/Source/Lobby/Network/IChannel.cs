using System;

namespace Mox.Lobby
{
    public interface IChannel
    {
        IAsyncResult<TResponse> BeginRequest<TResponse>(Message message)
            where TResponse : Message;

        TResponse Request<TResponse>(Message message)
            where TResponse : Message;

        void Send(Message message);

        event EventHandler<MessageReceivedEventArgs> MessageReceived;

#warning [Medium] Handle disconnection in client
        event EventHandler Disconnected;
    }

    public static class ChannelExtensions
    {
        public static void Respond(this IChannel channel, Message request, Message response)
        {
            response.RequestId = request.RequestId;
            channel.Send(response);
        }
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
