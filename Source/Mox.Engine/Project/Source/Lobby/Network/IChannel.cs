using System;

namespace Mox.Lobby
{
    public interface IChannel
    {
        TResponse Request<TResponse>(Message message)
            where TResponse : Message;

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
