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
using System.Linq;

namespace Mox.Flow.Parts
{
    /// <summary>
    /// Coordinates the high-level flow of a game (turns, winning/losing conditions).
    /// </summary>
    public class GameFlow : MTGPart
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameFlow(Player startingPlayer)
            : base(startingPlayer)
        {
        }

        #endregion

        #region Overrides of Part<IGameController>

        public override Part<IGameController> Execute(Context context)
        {
            Player startingPlayer = GetPlayer(context);

            foreach (Player player in Player.Enumerate(startingPlayer, false))
            {
                context.Schedule(new Mulligan(player));
            }

            context.Schedule(new MainPart(startingPlayer));
            return null;
        }

        #endregion
    }
}
