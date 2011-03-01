// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Runtime.Serialization;

namespace Mox.Lobby.Network
{
    /// <summary>
    /// Possible results of a login operation.
    /// </summary>
    public enum LoginResult
    {
        /// <summary>
        /// Login was successful.
        /// </summary>
        Success,
        /// <summary>
        /// Client is already logged to this server.
        /// </summary>
        AlreadyLoggedIn
    }

    [DataContract]
    public class LoginDetails
    {
        #region Variables

        [DataMember]
        private readonly LoginResult m_result;

        [DataMember]
        private readonly User m_user;

        [DataMember]
        private readonly Guid m_lobbyId;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public LoginDetails(LoginResult result, User user, Guid lobbyId)
        {
            m_result = result;
            m_user = user;
            m_lobbyId = lobbyId;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Result of the login operation.
        /// </summary>
        public LoginResult Result
        {
            get { return m_result; }
        }

        /// <summary>
        /// Client that corresponds to the login.
        /// </summary>
        public User User
        {
            get { return m_user; }
        }

        /// <summary>
        /// The id of the lobby that was logged into.
        /// </summary>
        public Guid LobbyId
        {
            get { return m_lobbyId; }
        }

        #endregion
    }
}
