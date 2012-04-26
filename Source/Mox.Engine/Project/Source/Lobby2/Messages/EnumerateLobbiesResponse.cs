using System;
using System.Collections.Generic;

namespace Mox.Lobby2
{
    public class EnumerateLobbiesResponse : Message
    {
        public IEnumerable<Guid> Lobbies
        {
            get;
            set;
        }
    }

    public class LogoutMessage : Message
    {}
}