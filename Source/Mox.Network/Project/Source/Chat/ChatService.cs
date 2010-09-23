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
using System.Collections.Generic;

namespace Mox.Network
{
    /// <summary>
    /// Concrete implementation of the <see cref="IChatService"/> service.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ChatService : SecondaryService, IChatPrivateService
    {
        #region Variables

        private readonly List<IChatClient> m_clients = new List<IChatClient>();
        private readonly Dictionary<string, string> m_chatSessionToMainSession = new Dictionary<string, string>();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public ChatService(IServiceManager serviceManager)
            : base(serviceManager)
        {
        }

        #endregion

        #region IChatService Members

        public bool Login(string serviceSessionId)
        {
            // Try to get the client associated with the given session id
            Client client = MainService.GetClient(serviceSessionId);
            if (client != null)
            {
                // found a proper client, add the current client
                m_clients.Add(OperationContext.GetCallbackChannel<IChatClient>());
                // map the current session to the main session
                m_chatSessionToMainSession.Add(OperationContext.SessionId, serviceSessionId);

                Log(new LogMessage { Importance = LogImportance.Debug, Text = string.Format("User {0} [{1}] logged in to chat service", client.Name, serviceSessionId) });

                return true;
            }

            return false;
        }

        public void Logout()
        {
            string chatSessionId = OperationContext.SessionId;
            if (m_chatSessionToMainSession.ContainsKey(chatSessionId))
            {
                string mainSessionId = m_chatSessionToMainSession[chatSessionId];
                Log(new LogMessage { Importance = LogImportance.Debug, Text = string.Format("User [{0}] logged out from chat service", mainSessionId) });

                m_clients.Remove(OperationContext.GetCallbackChannel<IChatClient>());
                m_chatSessionToMainSession.Remove(chatSessionId);
            }
        }

        public void Say(string message)
        {
            Client speaker = GetSpeaker();
            if (speaker != null)
            {
                Log(new LogMessage { Importance = LogImportance.Low, Text = string.Format("User {0} says: {1}", speaker.Name, message) });
                ForeachClient(client => client.ClientTalked(speaker, message));
            }
        }

        #endregion

        #region Methods

        private void ForeachClient(Action<IChatClient> action)
        {
            IChatClient[] copy = m_clients.ToArray();

            copy.ForEach(delegate(IChatClient client)
            {
                try
                {
                    action(client);
                }
                catch
                {
                    DropClient(client);
                }
            });
        }

        /// <summary>
        /// Disconnects the given client
        /// </summary>
        /// <param name="cur"></param>
        private void DropClient(IChatClient client)
        {
            // check if it is a known client
            if (m_clients.Remove(client))
            {
                // disconnect the client
                ICommunicationObject comObject = client as ICommunicationObject;
                if (comObject != null && comObject.State != CommunicationState.Closed)
                {
                    comObject.Abort();
                }
            }
        }

        private Client GetSpeaker()
        {
            string mainSessionId;
            if (!m_chatSessionToMainSession.TryGetValue(OperationContext.SessionId, out mainSessionId))
            {
                return null;
            }
            return MainService.GetClient(mainSessionId);
        }

        #endregion
    }
}
