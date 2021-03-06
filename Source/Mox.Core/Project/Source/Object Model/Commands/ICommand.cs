﻿// Copyright (c) François Paradis
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
    /// <summary>
    /// Interface for undoable commands.
    /// </summary>
    public interface ICommand
    {
        #region Properties

        /// <summary>
        /// Returns true if the command is empty (does nothing).
        /// </summary>
        bool IsEmpty
        {
            get;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes (does or redoes) the command.
        /// </summary>
        void Execute(ObjectManager objectManager);
        
        /// <summary>
        /// Unexecutes (undoes) the command.
        /// </summary>
        void Unexecute(ObjectManager objectManager);

        #endregion
    }
}
