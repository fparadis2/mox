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

namespace Mox.Flow.Parts
{
    /// <summary>
    /// A part that gives priority to players until they all pass and the stack is empty.
    /// </summary>
    public class PlayUntilAllPlayersPassAndTheStackIsEmpty : PlayerPart
    {
        #region Inner Types

        private class SubImplementation : PlayUntilAllPlayersPassAndTheStackIsEmpty
        {
            #region Constructor

            public SubImplementation(Player player) : base(player)
            {
            }

            #endregion

            #region Overrides of Part

            public override Part Execute(Context context)
            {
                if (context.Game.State.HasEnded || context.Game.SpellStack.IsEmpty)
                {
                    return null;
                }

                context.Schedule(new ResolveTopSpell());

                return base.Execute(context);
            }

            #endregion
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="player">Player to give priority to first.</param>
        public PlayUntilAllPlayersPassAndTheStackIsEmpty(Player player)
            : base(player)
        {
        }

        #endregion

        #region Overrides of Part

        public override Part Execute(Context context)
        {
            Player player = GetPlayer(context);

            context.Schedule(new PlayUntilAllPlayersPass(player));
            return new SubImplementation(player);
        }

        #endregion
    }
}
