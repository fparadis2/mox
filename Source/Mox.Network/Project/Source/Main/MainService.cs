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
    /// The main "game" service.
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
    public partial class MainService : IMainService
    {
        #region Variables

        private readonly IOperationContext m_operationContext;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="operationContext"></param>
        public MainService(IOperationContext operationContext)
        {
            Throw.IfNull(operationContext, "operationContext");
            m_operationContext = operationContext;

            m_clients = new ClientInfoCollection(this);
        }

        #endregion

        #region Properties

        private IOperationContext OperationContext
        {
            get { return m_operationContext; }
        }

        #endregion

        #region Methods

        private IMoxClient GetCurrentClient()
        {
            return OperationContext.GetCallbackChannel<IMoxClient>();
        }

        #endregion
    }
}
