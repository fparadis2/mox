﻿using System;
using System.Collections.Generic;

namespace Mox.Lobby
{
    [Serializable]
    public class EnumerateLobbiesResponse : Message
    {
        public IList<Guid> Lobbies
        {
            get;
            set;
        }
    }
}