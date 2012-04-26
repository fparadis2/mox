using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mox.Lobby2
{
    public class EnterLobbyRequest : Message
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
