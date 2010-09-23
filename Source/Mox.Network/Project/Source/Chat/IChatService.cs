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

namespace Mox.Network
{
    /// <summary>
    /// Chat service interface.
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IChatService
    {
        #region Methods

        /// <summary>
        /// Called by the client to speak on behalf of the user
        /// </summary>
        /// <param name="message">The user message</param>
        [OperationContract(IsOneWay = true)]
        void Say(string message);

        #endregion
    }

    /// <summary>
    /// Chat service private interface (interface not meant to be used directly by clients).
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IChatClient), SessionMode = SessionMode.Required)]
    public interface IChatPrivateService : IChatService
    {
        #region Methods

        /// <summary>
        /// Tries to login the current client.
        /// </summary>
        /// <param name="serviceSessionId">The session id of a valid session with the <see cref="IMoxService"/></param>
        /// <returns>True if the client logged in and can chat, false otherwise</returns>
        /// <remarks>
        /// The session id which is passed to <paramref name="serviceSessionId"/> can be retrieved by the <see cref="IContextChannel.SessionId"/> property.
        /// </remarks>
        [OperationContract(IsInitiating = true)]
        bool Login(string serviceSessionId);

        /// <summary>
        /// Logouts the current client out of the chat room
        /// </summary>
        [OperationContract(IsTerminating = true)]
        void Logout();

        #endregion
    }
}
