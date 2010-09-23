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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mox.Network
{
    /// <summary>
    /// Client Info.
    /// </summary>
    public class ClientInfo
    {
        #region Variables

        private readonly string m_sessionId;
        private readonly Client m_client;
        private readonly IMoxClient m_clientInterface;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="client"></param>
        /// <param name="clientInterface"></param>
        public ClientInfo(string sessionId, Client client, IMoxClient clientInterface)
        {
            Throw.IfEmpty(sessionId, "sessionId");
            Throw.IfNull(client, "client");
            Throw.IfNull(clientInterface, "clientInterface");

            m_sessionId = sessionId;
            m_client = client;
            m_clientInterface = clientInterface;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Session Id.
        /// </summary>
        public string SessionId
        {
            get { return m_sessionId; }
        }

        /// <summary>
        /// Client.
        /// </summary>
        public Client Client
        {
            get { return m_client; }
        }

        /// <summary>
        /// Client Interface.
        /// </summary>
        public IMoxClient ClientInterface
        {
            get { return m_clientInterface; }
        }

        #endregion
    }
}
