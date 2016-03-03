﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mox.Lobby
{
    [Serializable]
    public class EnterLobbyRequest : Request<JoinLobbyResponse>
    {
        #region Properties

        public Guid LobbyId
        {
            get;
            set;
        }

        public string Username
        {
            get;
            set;
        }

        #endregion
    }
}
