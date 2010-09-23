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

namespace Mox.Network
{
    /// <summary>
    /// Helper class.
    /// </summary>
    public static class ServiceUtilities
    {
        #region Constants

        /// <summary>
        /// Network Constants.
        /// </summary>
        public static class Constants
        {
            /// <summary>
            /// Main service.
            /// </summary>
            public const string MoxServiceName = "MoxService";

            /// <summary>
            /// Chat service.
            /// </summary>
            public const string ChatServiceName = "ChatService";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the service endpoint address corresponding to the given <paramref name="serviceName"/>, <paramref name="hostOrIp"/> and <paramref name="port"/>.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static string GetServiceAddress(string serviceName, string hostOrIp, int port)
        {
            return string.Format("net.tcp://{0}:{1}/{2}", hostOrIp, port, serviceName);
        }

        #endregion
    }
}
