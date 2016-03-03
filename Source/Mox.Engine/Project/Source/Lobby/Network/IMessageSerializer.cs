using System;
using System.IO;

namespace Mox.Lobby
{
#warning delete
    internal interface IMessageSerializer
    {
        MemoryStream WriteMessage(Message message);
        Message ReadMessage(Stream stream);
    }
}
