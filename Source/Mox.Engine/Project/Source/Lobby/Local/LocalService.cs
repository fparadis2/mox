using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mox.Lobby
{
    internal abstract class LocalService
    {
        #region Variables

        private readonly LocalLobby m_owner;

        #endregion

        #region Constructor

        protected LocalService(LocalLobby owner)
        {
            m_owner = owner;
        }

        #endregion

        #region Properties

        protected User User
        {
            get { return m_owner.User; }
        }

        protected LocalLobby Owner
        {
            get { return m_owner; }
        }

        #endregion
    }
}
