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

using Mox.Lobby;

namespace Mox.Network
{
    /// <summary>
    /// Concrete implementation of the <see cref="INetworkChatService"/> service.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class NetworkChatService : SecondaryService, INetworkChatPrivateService
    {
        #region Inner Types

        private class NetworkChatClient : IChatClient
        {
            #region Variables

            private readonly INetworkChatClient m_innerClient;

            #endregion

            #region Constructor

            public NetworkChatClient(INetworkChatClient innerClient)
            {
                m_innerClient = innerClient;
            }

            #endregion

            #region Methods

            public void OnMessageReceived(User user, string message)
            {
                m_innerClient.OnMessageReceived(user, message);
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly ChatService m_innerService = new ChatService();
        private readonly Dictionary<string, string> m_chatSessionToMainSession = new Dictionary<string, string>();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public NetworkChatService(IServiceManager serviceManager)
            : base(serviceManager)
        {
        }

        #endregion

        #region INetworkChatService Members

        public bool Login(string serviceSessionId)
        {
            // Try to get the client associated with the given session id
            User user = MainService.GetClient(serviceSessionId);
            if (user != null)
            {
#warning [Network] Use correct chat level
                // found a proper client, add the current client
                var proxyClient = new NetworkChatClient(OperationContext.GetCallbackChannel<INetworkChatClient>());
                m_innerService.Register(user, proxyClient, ChatLevel.Normal);
                // map the current session to the main session
                m_chatSessionToMainSession.Add(OperationContext.SessionId, serviceSessionId);

                Log(new LogMessage { Importance = LogImportance.Debug, Text = string.Format("User {0} [{1}] logged in to chat service", user.Name, serviceSessionId) });

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

#warning todo
                //m_clients.Remove(OperationContext.GetCallbackChannel<IChatClient>());
                m_chatSessionToMainSession.Remove(chatSessionId);
            }
        }

        public void Say(string message)
        {
            User speaker = GetSpeaker();
            if (speaker != null)
            {
                Log(new LogMessage { Importance = LogImportance.Low, Text = string.Format("User {0} says: {1}", speaker.Name, message) });
                ForeachClient(client => client.OnMessageReceived(speaker, message));
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

        private User GetSpeaker()
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
