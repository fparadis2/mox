using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Mox.Lobby.Network
{
    internal class TcpChannel : ChannelBase
    {
        #region Variables

        private readonly TcpClient m_client;
        private readonly NetworkStream m_stream;
        private readonly MemoryStream m_sendMessageStream = new MemoryStream();
        private readonly MessageQueue m_sendQueue;

        private bool m_disconnected;

        #endregion

        #region Constructor

        protected TcpChannel(TcpClient client, MessageQueue sendQueue)
        {
            m_client = client;
            m_stream = client.GetStream();
            m_sendQueue = sendQueue;

            ReceiveMessages();
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

        protected override void SendMessage(Message message)
        {
            m_sendQueue.Enqueue(() => OnSendMessage(message));
        }

        private void OnSendMessage(Message message)
        {
            m_sendMessageStream.SetLength(0);
            m_sendMessageStream.Seek(0, SeekOrigin.Begin);

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(m_sendMessageStream, message);

            int messageLength = (int)m_sendMessageStream.Length;
            byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
            m_stream.Write(messageLengthBytes, 0, messageLengthBytes.Length);
            m_stream.Write(m_sendMessageStream.GetBuffer(), 0, messageLength);
        }

        #endregion

        #region Receive

        private async void ReceiveMessages()
        {
            byte[] receiveBuffer = new byte[1024];

            while (true)
            {
                int messageSize = await ReceiveMessageSize(receiveBuffer);
                GrowIfNeeded(ref receiveBuffer, messageSize);
                bool receivedMessage = await ReceiveMessage(receiveBuffer, messageSize);

                if (!receivedMessage)
                {
                    Close();
                    return;
                }

                var message = DeserializeMessage(receiveBuffer, messageSize);
                OnMessageReceived(message);
            }
        }

        private async Task<int> ReceiveMessageSize(byte[] receiveBuffer)
        {
            int readBytes = await m_stream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);
            if (readBytes <= 0)
            {
                return 0;
            }

            Debug.Assert(readBytes == Marshal.SizeOf(typeof(int)));
            return BitConverter.ToInt32(receiveBuffer, 0);
        }

        private async Task<bool> ReceiveMessage(byte[] receiveBuffer, int messageSize)
        {
            if (messageSize <= 0)
                return false;

            int remainingSize = messageSize;
            while (remainingSize > 0)
            {
                int readBytes = await m_stream.ReadAsync(receiveBuffer, messageSize - remainingSize, remainingSize);
                if (readBytes <= 0)
                {
                    return false;
                }
                remainingSize -= readBytes;
                Debug.Assert(remainingSize >= 0);
            }

            return true;
        }

        private Message DeserializeMessage(byte[] receiveBuffer, int messageSize)
        {
            using (MemoryStream stream = new MemoryStream(receiveBuffer, 0, messageSize))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (Message)formatter.Deserialize(stream);
            }
        }

        private void GrowIfNeeded(ref byte[] buffer, int size)
        {
            if (buffer.Length < size)
            {
                buffer = new byte[(int)(size * 1.2)];
            }
        }

        #endregion

        #endregion
    }
}
