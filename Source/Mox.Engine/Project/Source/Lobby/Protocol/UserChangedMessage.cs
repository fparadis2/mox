using System;
using System.Collections.Generic;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public class UserJoinedMessage : Message
    {
        public Guid UserId;
        public UserData Data;
    }

    [Serializable]
    public class UserLeftMessage : Message
    {
        public Guid UserId;
    }
}
