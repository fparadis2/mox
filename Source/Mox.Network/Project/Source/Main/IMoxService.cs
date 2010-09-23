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
    /// The main network service.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IMoxClient), SessionMode = SessionMode.Required)]
    [DeliveryRequirements(RequireOrderedDelivery = true)]
    public interface IMoxService
    {
        #region Login

        /// <summary>
        /// Tries to login the client with the given user name.
        /// </summary>
        /// <param name="userName">The name to sign in the client</param>
        /// <returns>
        /// A valid client when the sign in succeeds. Null value is returned in case of an error (after a proper callback)
        /// </returns>
        [OperationContract(IsInitiating = true)]
        LoginDetails Login(string userName);

        /// <summary>
        /// Request the server to logout from the current game
        /// </summary>
        [OperationContract(IsTerminating = true)]
        void Logout();

        #endregion
    }
}
