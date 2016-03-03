using System;

namespace Mox.Lobby
{
    [Serializable]
    public class Message
    {
    }

    [Serializable]
    public class Request : Message
    {
        internal ushort RequestId;
    }

    [Serializable]
    public class Request<TResponse> : Request
        where TResponse : Response
    {
    }

    [Serializable]
    public class Response : Message
    {
        internal ushort RequestId;
    }
}
