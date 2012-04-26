using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mox.Lobby
{
    [Serializable]
    public class CreateLobbyRequest : Message
    {
        #region Properties

        public string Username
        {
            get;
            set;
        }

        #endregion
    }
}
