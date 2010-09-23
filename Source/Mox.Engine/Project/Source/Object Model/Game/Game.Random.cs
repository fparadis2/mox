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

namespace Mox
{
    partial class Game
    {
        #region Variables

        private IRandom m_random;

        #endregion

        #region Properties

        /// <summary>
        /// Events of the game.
        /// </summary>
        public IRandom Random
        {
            get
            {
                Throw.InvalidOperationIf(m_random == null, "Cannot access the random on this game instance. Are you operating on a replicated game?");
                return m_random;
            }
        }

        #endregion

        #region Methods

        public IDisposable UseRandom(IRandom random)
        {
            IRandom oldRandom = m_random;
            m_random = random;

            return new DisposableHelper(() => m_random = oldRandom);
        }

        #endregion
    }
}
