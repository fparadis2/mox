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
using System.Diagnostics;
using System.Linq;

using Mox.Transactions;

namespace Mox.AI
{
    public abstract class BaseMinMaxAlgorithm : IMinMaxAlgorithm
    {
        #region Constants

        /// <summary>
        /// Max legal heuristic value.
        /// </summary>
        public const float MaxValue = MinimaxTree.MaxValue;

        /// <summary>
        /// Min legal heuristic value.
        /// </summary>
        public const float MinValue = MinimaxTree.MinValue;

        #endregion

        #region Variables

        private readonly int m_maximizingPlayerIdentifier;
        private readonly AIParameters m_parameters;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        protected BaseMinMaxAlgorithm(Player maximizingPlayer, AIParameters parameters)
        {
            Throw.IfNull(maximizingPlayer, "maximizingPlayer");
            Throw.IfNull(parameters, "parameters");

            m_maximizingPlayerIdentifier = maximizingPlayer.Identifier;
            m_parameters = parameters;
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BaseMinMaxAlgorithm()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        #endregion

        #region Properties

        protected AIParameters Parameters
        {
            get { return m_parameters; }
        }

        #endregion

        #region Implementation of IMinMaxAlgorithm

        /// <summary>
        /// Returns true if the current search should be stopped at this depth.
        /// </summary>
        /// <remarks>
        /// Normally this would return true if the game has ended, or if the search reached a specific depth.
        /// </remarks>
        /// <param name="tree"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public abstract bool IsTerminal(IMinimaxTree tree, Game game);

        /// <summary>
        /// Computes the "value" of the current "game state".
        /// </summary>
        /// <returns></returns>
        public abstract float ComputeHeuristic(Game game, bool considerGameEndingState);

        /// <summary>
        /// Returns true if the given <paramref name="player"/> is the maximizing player.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool IsMaximizingPlayer(Player player)
        {
            if (player == null)
            {
                return false;
            }

            return player.Identifier == m_maximizingPlayerIdentifier;
        }

        #endregion
    }
}
