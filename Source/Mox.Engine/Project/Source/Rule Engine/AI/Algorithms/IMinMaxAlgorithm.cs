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

using Mox.Transactions;

namespace Mox.AI
{
    /// <summary>
    /// Drives the min max algorithm.
    /// </summary>
    public interface IMinMaxAlgorithm : IDisposable
    {
        #region Methods

        /// <summary>
        /// Returns true if the current search should be stopped at this depth.
        /// </summary>
        /// <remarks>
        /// Normally this would return true if the game has ended, or if the search reached a specific depth.
        /// </remarks>
        /// <param name="tree"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        bool IsTerminal(IMinimaxTree tree, Game game);

        /// <summary>
        /// Computes the "value" of the current "game state".
        /// </summary>
        /// <returns></returns>
        float ComputeHeuristic(Game game, bool considerGameEndingState);

        /// <summary>
        /// Returns true if the given <paramref name="player"/> is the maximizing player.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        bool IsMaximizingPlayer(Player player);

        #endregion
    }
}
