using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Mox.Lobby
{
    internal class MessageSerializer : IMessageSerializer
    {
        #region Implementation of IMessageSerializer

        public MemoryStream WriteMessage(Message message)
        {
            MemoryStream stream = new MemoryStream();

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, message);

            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public Message ReadMessage(Stream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return (Message)formatter.Deserialize(stream);
        }

        #endregion
    }
}
