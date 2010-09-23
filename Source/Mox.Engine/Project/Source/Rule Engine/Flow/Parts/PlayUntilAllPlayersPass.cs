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

namespace Mox.Flow.Parts
{
    /// <summary>
    /// A part that gives priority to players until they all pass.
    /// </summary>
    public class PlayUntilAllPlayersPass : MTGPart
    {
        #region Variables

        private readonly int m_numPlayerPassed;
        private readonly bool m_checkLastMove;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        private PlayUntilAllPlayersPass(Player player, int numPlayersPassed, bool checkLastMove)
            : base(player)
        {
            m_numPlayerPassed = numPlayersPassed;
            m_checkLastMove = checkLastMove;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="player">Player to give priority to.</param>
        public PlayUntilAllPlayersPass(Player player)
            : this(player, 0, false)
        {
        }

        #endregion

        #region Properties

        #endregion

        #region Overrides of Part

        public override Part<IGameController> Execute(Context context)
        {
            bool isSamePlayerThanPrevious = !m_checkLastMove || !context.PopArgument<bool>(GivePriority.ArgumentToken);

            if (context.Game.State.HasEnded)
            {
                return null;
            }

            int numPlayersPassed = 0;
            Player nextPlayer = GetPlayer(context);
            
            if (!isSamePlayerThanPrevious)
            {
                numPlayersPassed = m_numPlayerPassed + 1;
                nextPlayer = Player.GetNextPlayer(nextPlayer);
            }

            if (numPlayersPassed >= context.Game.Players.Count)
            {
                return null;
            }

            context.Schedule(new CheckStateBasedActions());
            context.Schedule(new HandleTriggeredAbilities(nextPlayer));
            context.Schedule(new GivePriority(nextPlayer));

            return new PlayUntilAllPlayersPass(nextPlayer, numPlayersPassed, true);
        }

        #endregion
    }
}
