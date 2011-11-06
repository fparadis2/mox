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
    /// Handles the main part of the game, scheduling turns for each player in sequence.
    /// </summary>
    public class MainPart : PlayerPart
    {
        #region Constructor

        public MainPart(Player player)
            : base(player)
        {
        }

        #endregion

        #region Overrides of Part<IGameController>

        public override NewPart Execute(Context context)
        {
            Player player = GetPlayer(context);

            context.Schedule(new SequenceTurn(player));
            return new MainPart(Player.GetNextPlayer(player));
        }

        #endregion
    }
}
