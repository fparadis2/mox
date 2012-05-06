using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Mox.Lobby
{
    internal class TcpChannel : ChannelBase
    {
        #region Variables

        private readonly TcpClient m_client;
        private readonly NetworkStream m_stream;
        private readonly IMessageSerializer m_serializer;
        private readonly MessageQueue m_sendQueue;

        private byte[] m_receiveBuffer = new byte[1024];

        private bool m_disconnected;

        #endregion

        #region Constructor

        protected TcpChannel(TcpClient client, IMessageSerializer serializer, MessageQueue sendQueue)
        {
            m_client = client;
            m_stream = client.GetStream();
            m_serializer = serializer;
            m_sendQueue = sendQueue;

            BeginReceiveHeader();
        }

        #endregion

        #region Methods

        #region Connection

        public void Close()
        {
            if (!m_disconnected)
            {
                lock (m_client)
                {
                    if (!m_disconnected)
                    {
                        // TODO: What happens with receive queue?

                        m_sendQueue.Join(); // Wait for sends to finish
                        m_client.Close();

                        OnDisconnected();

                        m_disconnected = true;
                    }
                }
            }
        }

        #endregion

        #region Send

        public override void Send(Message message)
        {
            m_sendQueue.Enqueue(() => OnSendMessage(message));
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

        private void WhenReceiveHeader(System.IAsyncResult result)
        {
            int readBytes;
            if (!TryEndRead(result, out readBytes))
            {
                return;
            }

            Debug.Assert(readBytes == Marshal.SizeOf(typeof (int)));

            int messageSize = BitConverter.ToInt32(m_receiveBuffer, 0);
            BeginReceiveMessage(new MessageInfo(messageSize));
        }

        private void WhenReceiveMessage(System.IAsyncResult result)
        {
            int readBytes;
            if (!TryEndRead(result, out readBytes))
            {
                return;
            }

            MessageInfo messageInfo = (MessageInfo)result.AsyncState;
            Debug.Assert(readBytes <= messageInfo.RemainingSize);
            messageInfo.RemainingSize -= readBytes;

            if (messageInfo.RemainingSize == 0)
            {
                // Message is complete
                ReadMessage(messageInfo);
            }
            else
            {
                BeginReceiveMessage(messageInfo);
            }
        }

        private void ReadMessage(MessageInfo messageInfo)
        {
            Message message;

            using (MemoryStream stream = new MemoryStream(m_receiveBuffer, 0, messageInfo.MessageSize))
            {
                message = m_serializer.ReadMessage(stream);
            }

            if (ReceptionDispatcher.ReceiveMessagesSynchronously)
            {
                OnMessageReceived(message);
                BeginReceiveHeader();
            }
            else
            {
                BeginReceiveHeader();
                OnMessageReceived(message);
            }
        }

        private bool TryEndRead(System.IAsyncResult result, out int readBytes)
        {
            readBytes = 0;

            try
            {
                readBytes = m_stream.EndRead(result);
            }
            catch {}

            if (readBytes == 0)
            {
                Close();
                return false;
            }

            return true;
        }

        #endregion

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
