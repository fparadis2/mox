using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Mox.Lobby2
{
    internal class TcpChannel : IChannel
    {
        #region Variables

        private readonly NetworkStream m_stream;
        private readonly IMessageSerializer m_serializer;
        private readonly MessageQueue m_sendQueue;

        private byte[] m_receiveBuffer = new byte[1024];
        private PendingRequest m_pendingRequest;

        #endregion

        #region Constructor

        protected TcpChannel(TcpClient client, IMessageSerializer serializer, MessageQueue sendQueue)
        {
            m_stream = client.GetStream();
            m_serializer = serializer;
            m_sendQueue = sendQueue;

            BeginReceiveHeader();
        }

        #endregion

        #region Methods

        #region Send

        public TResponse Request<TResponse>(Message message) where TResponse : Message
        {
            PendingRequest<TResponse> request = new PendingRequest<TResponse>();

            try
            {
                Debug.Assert(m_pendingRequest == null, "TODO");
                m_pendingRequest = request;

                Send(message);

                return request.Consume();
            }
            finally
            {
                m_pendingRequest = null;
            }
        }

        public void Send(Message message)
        {
            m_sendQueue.Enqueue(message, OnSendMessage);
        }

        protected void OnSendMessage(Message message)
        {
            using (MemoryStream stream = m_serializer.WriteMessage(message))
            {
                int messageLength = (int)stream.Length;
                byte[] messageHeader = BitConverter.GetBytes(messageLength);

                m_stream.Write(messageHeader, 0, messageHeader.Length);
                m_stream.Write(stream.GetBuffer(), 0, (int)stream.Length);
            }
        }

        #endregion

        #region Receive

        private void BeginReceiveHeader()
        {
            m_stream.BeginRead(m_receiveBuffer, 0, Marshal.SizeOf(typeof(int)), WhenReceiveHeader, null);
        }

        private void BeginReceiveMessage(MessageInfo messageInfo)
        {
            int sizeToRead = messageInfo.RemainingSize;

            if (m_receiveBuffer.Length < sizeToRead)
            {
                m_receiveBuffer = new byte[(int)(sizeToRead * 1.2)];
            }

            m_stream.BeginRead(m_receiveBuffer, messageInfo.MessageSize - sizeToRead, sizeToRead, WhenReceiveMessage, messageInfo);
        }

        private void WhenReceiveHeader(IAsyncResult result)
        {
            int readBytes = m_stream.EndRead(result);
            Debug.Assert(readBytes == Marshal.SizeOf(typeof (int)));

            int messageSize = BitConverter.ToInt32(m_receiveBuffer, 0);
            BeginReceiveMessage(new MessageInfo(messageSize));
        }

        private void WhenReceiveMessage(IAsyncResult result)
        {
            MessageInfo messageInfo = (MessageInfo)result.AsyncState;
            int readBytes = m_stream.EndRead(result);

            Debug.Assert(readBytes <= messageInfo.RemainingSize);
            messageInfo.RemainingSize -= readBytes;

            if (messageInfo.RemainingSize == 0)
            {
                // Message is complete
                ReadMessage(messageInfo);
                BeginReceiveHeader();
            }
            else
            {
                BeginReceiveMessage(messageInfo);
            }
        }

        private void ReadMessage(MessageInfo messageInfo)
        {
            using (MemoryStream stream = new MemoryStream(m_receiveBuffer, 0, messageInfo.MessageSize))
            {
                Message message = m_serializer.ReadMessage(stream);

                var pendingRequest = m_pendingRequest;
                if (pendingRequest == null || !pendingRequest.Consider(message))
                {
                    OnReadMessage(message, OnMessageReceived);
                }
            }
        }

        /// <summary>
        /// 1. On server: process synchronously (right away)
        /// 2. On client: queue and fire up a wake up job on the main thread
        /// </summary>
        protected virtual void OnReadMessage(Message message, Action<Message> readMessage)
        {
            readMessage(message);
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        private void OnMessageReceived(Message message)
        {
            MessageReceived.Raise(this, new MessageReceivedEventArgs(message));
        }

        public event EventHandler Disconnected;

        #endregion

        #region Inner Types

        private struct MessageInfo
        {
            public MessageInfo(int messageSize)
            {
                MessageSize = messageSize;
                RemainingSize = messageSize;
            }

            public readonly int MessageSize;
            public int RemainingSize;
        }

        #endregion
    }
}
