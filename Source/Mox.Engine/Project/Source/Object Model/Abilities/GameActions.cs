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

namespace Mox
{
    /// <summary>
    /// Extensions methods for some common game actions.
    /// </summary>
    public static class GameActions
    {
        #region Player extensions

        /// <summary>
        /// Draws card(s) from the top of the library.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="numCards">Number of cards to draw.</param>
        public static void DrawCards(this Player player, int numCards)
        {
            if (player.Library.Count < numCards)
            {
                player.HasDrawnMoreCardsThanAvailable = true;
                numCards = player.Library.Count;
            }

            int cardsToDraw = numCards;
            while (cardsToDraw-- > 0)
            {
                Card drawn = player.Library.Top();
                drawn.Zone = player.Manager.Zones.Hand;
                player.Manager.Events.Trigger(new Events.DrawCardEvent(player, drawn));
            }

            player.Manager.Log.Log(player, $"Drew {numCards} {numCards.Pluralize("card")}.");
        }

        /// <summary>
        /// Discards the given <paramref name="card"/>.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="card"></param>
        public static void Discard(this Player player, Card card)
        {
            Debug.Assert(player.Hand.Contains(card), "Card is not in player hand!");

            player.Graveyard.MoveToTop(new[] { card });

            player.Manager.Events.Trigger(new Events.PlayerDiscardedEvent(player, card));

            player.Manager.Log.Log(player, $"Discarded {card}.");
        }

        #endregion
    }
}
