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
using System.Threading;

namespace Mox.UI
{
    /// <summary>
    /// An interaction between the user and the game.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IInteraction<T>
    {
        #region Properties

        /// <summary>
        /// The result of the interaction, or null/default value if the operation has not completed.
        /// </summary>
        T Result
        {
            get;
        }

        /// <summary>
        /// A wait handle that can be used to wait for the operation to finish.
        /// </summary>
        WaitHandle WaitHandle
        {
            get;
        }

        /// <summary>
        /// Returns true if the interaction has completed.
        /// </summary>
        bool IsCompleted { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Stops the calling thread until the interaction completes.
        /// </summary>
        void Wait();

        #endregion
    }
}