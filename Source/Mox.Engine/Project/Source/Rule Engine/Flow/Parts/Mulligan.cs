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
using System.Text;

namespace Mox.Flow.Parts
{
    /// <summary>
    /// Handles the mulligan part for a given player.
    /// </summary>
    public class Mulligan : MTGPart
    {
        #region Variables

        private readonly int m_numCards;

        #endregion

        #region Constructor

        public Mulligan(Player player)
            : this(player, -1)
        {
        }

        private Mulligan(Player player, int numCards)
            : base(player)
        {
            m_numCards = numCards;
        }

        #endregion

        #region Overrides of Part<IGameController>

        private int GetNumCards(Player player)
        {
            return m_numCards < 0 ? player.MaximumHandSize : m_numCards;
        }

        public override ControllerAccess ControllerAccess
        {
            get
            {
                return ControllerAccess.Single;
            }
        }

        public override Part<IGameController> Execute(Context context)
        {
            Player player = GetPlayer(context);
            int newNumCards = Math.Min(GetNumCards(player), player.Library.Count);

            using (context.Game.Controller.BeginCommandGroup())
            {
                // Move all player's cards into his library
                player.Library.MoveToTop(player.Hand);
                player.Library.Shuffle();

                // Draw X new cards
                player.DrawCards(newNumCards);
            }

            if (player.Hand.Count > 0 && context.Controller.Mulligan(context, player))
            {
                return new Mulligan(player, newNumCards - 1);
            }

            return null;
        }

        #endregion
    }
}
