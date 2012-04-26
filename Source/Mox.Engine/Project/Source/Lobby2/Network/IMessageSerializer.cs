using System;
using System.IO;

namespace Mox.Lobby2
{
    internal interface IMessageSerializer
    {
        MemoryStream WriteMessage(Message message);
        Message ReadMessage(Stream stream);
    }
}
