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

namespace Mox.Transactions
{
    public interface ISynchronizableCommand
    {
        #region Properties

        /// <summary>
        /// True if the command is always visible to all users.
        /// </summary>
        bool IsPublic
        {
            get;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Object associated with the synchronizable command, if any.
        /// </summary>
        Object GetObject(ObjectManager objectManager);

        /// <summary>
        /// Gets the synchronization command for this command (usually the command itself).
        /// </summary>
        ICommand Synchronize();

        #endregion
    }
}
