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
    /// A collection of cards
    /// </summary>
    public interface ICardCollection : IList<Card>
    {
        /// <summary>
        /// Moves the given <paramref name="cards"/> to the top of this zone, in the order they are given (first cards are moved first).
        /// </summary>
        /// <param name="cards"></param>
        void MoveToTop(IEnumerable<Card> cards);

        /// <summary>
        /// Moves the given <paramref name="cards"/> to the bottom of this zone, in the order they are given (first cards are moved first).
        /// </summary>
        /// <param name="cards"></param>
        void MoveToBottom(IEnumerable<Card> cards);

        /// <summary>
        /// Shuffles the cards contained in this zone, for the current player.
        /// </summary>
        void Shuffle();
    }

    public static class CardCollectionExtensions
    {
        #region Top & Bottom

        public static Card Top(this ICardCollection collection)
        {
            return Top(collection, 1).Single();
        }

        public static IEnumerable<Card> Top(this ICardCollection collection, int count)
        {
            for (int i = collection.Count - 1 ; i >= 0 && count > 0; i--, count--)
            {
                yield return collection[i];
            }
        }

        public static Card Bottom(this ICardCollection collection)
        {
            return Bottom(collection, 1).Single();
        }

        public static IEnumerable<Card> Bottom(this ICardCollection collection, int count)
        {
            return collection.Take(count);
        }

        #endregion
    }
}
