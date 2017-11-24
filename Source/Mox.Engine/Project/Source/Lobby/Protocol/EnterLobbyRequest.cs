using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mox.Lobby.Network.Protocol
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

        public IUserIdentity Identity
        {
            get;
            set;
        }

        #endregion
    }
}
