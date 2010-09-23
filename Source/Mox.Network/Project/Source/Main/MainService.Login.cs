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
using System.ServiceModel;
using System.Collections.ObjectModel;

namespace Mox.Network
{
    partial class MainService
    {
        #region Inner Types

        private class ClientInfoCollection : KeyedCollection<string, ClientInfo>
        {
            #region Variables

            private readonly MainService m_owner;

            #endregion

            #region Constructor

            public ClientInfoCollection(MainService owner)
            {
                m_owner = owner;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Client associated with the calling method, if any.
            /// </summary>
            public ClientInfo CurrentClient
            {
                get
                {
                    string sessionId = m_owner.OperationContext.SessionId;
                    if (Contains(sessionId))
                    {
                        return this[sessionId];
                    }
                    return null;
                }
            }
            
            #endregion

            #region Methods

            protected override string GetKeyForItem(ClientInfo item)
            {
                return item.SessionId;
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly ClientInfoCollection m_clients;

        #endregion

        #region Methods
        /// <summary>
        /// Returns the client with the given <paramref name="mainSessionId"/>.
        /// </summary>
        /// <param name="mainSessionId"></param>
        /// <returns></returns>
        public Client GetClient(string mainSessionId)
        {
            if (!string.IsNullOrEmpty(mainSessionId) && m_clients.Contains(mainSessionId))
            {
                return m_clients[mainSessionId].Client;
            }

            return null;
        }

        /// <summary>
        /// Login.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public LoginDetails Login(string userName)
        {
            ClientInfo clientInfo = m_clients.CurrentClient;
            if (clientInfo != null)
            {
                // Already connected.
                return new LoginDetails(LoginResult.AlreadyLoggedIn, clientInfo.Client);
            }
            else
            {
                // Create the client 
                clientInfo = new ClientInfo(OperationContext.SessionId, new Client(userName), GetCurrentClient());
                m_clients.Add(clientInfo);
                return new LoginDetails(LoginResult.Success, clientInfo.Client);
            }
        }

        /// <summary>
        /// Logout
        /// </summary>
        public void Logout()
        {
            ClientInfo clientInfo = m_clients.CurrentClient;
            if (clientInfo != null)
            {
                m_clients.Remove(clientInfo);
            }
        }

        #endregion
    }
}
