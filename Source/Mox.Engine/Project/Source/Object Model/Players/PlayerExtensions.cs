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

namespace Mox
{
    /// <summary>
    /// Contains extension methods for players.
    /// </summary>
    public static class PlayerExtensions
    {
        #region Methods

        /// <summary>
        /// Lose life.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="life"></param>
        public static void LoseLife(this Player player, int life)
        {
            Throw.ArgumentOutOfRangeIf(life < 0, "Life losed must be positive", "life");
            player.Life -= life;
        }

        /// <summary>
        /// Gain life.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="life"></param>
        public static void GainLife(this Player player, int life)
        {
            Throw.ArgumentOutOfRangeIf(life < 0, "Life gained must be positive", "life");
            player.Life += life;
        }

        #endregion
    }
}
