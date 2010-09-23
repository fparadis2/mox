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
    /// Base class for services that depend on the <see cref="IMainService"/>.
    /// </summary>
    public abstract class SecondaryService
    {
        #region Variables

        private readonly IServiceManager m_serviceManager;

        #endregion

        #region Constructor

        protected SecondaryService(IServiceManager serviceManager)
        {
            Throw.IfNull(serviceManager, "serviceManager");
            m_serviceManager = serviceManager;
        }

        #endregion

        #region Properties

        protected IOperationContext OperationContext
        {
            get { return m_serviceManager.OperationContext; }
        }

        /// <summary>
        /// Main Service.
        /// </summary>
        protected IMainService MainService
        {
            get { return m_serviceManager.MainService; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Logs a message.
        /// </summary>
        protected void Log(LogMessage message)
        {
            m_serviceManager.Log(message);
        }

        #endregion
    }
}
