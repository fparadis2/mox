using System;
using System.IO;

namespace Mox.Lobby
{
    internal interface IMessageSerializer
    {
        MemoryStream WriteMessage(Message message);
        Message ReadMessage(Stream stream);
    }
}
