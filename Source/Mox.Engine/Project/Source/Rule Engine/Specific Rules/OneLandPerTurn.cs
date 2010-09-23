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

namespace Mox.Rules
{
    /// <summary>
    /// Implements the "one land per turn" rule
    /// </summary>
    public static class OneLandPerTurn
    {
        #region Variables

        private static readonly Property<int> NumberOfLandsPlayedThisTurn = Property<int>.RegisterAttachedProperty("NumberOfLandsPlayedThisTurn", typeof(OneLandPerTurn));
        private static readonly Scope m_bypassScope = new Scope();

        #endregion

        #region Methods

        /// <summary>
        /// Notifies this rule that a land has been played.
        /// </summary>
        /// <param name="game"></param>
        public static void PlayOneLand(Game game)
        {
            int currentLandsPlayed = GetNumLandsPlayedThisTurn(game);
            game.TurnData.SetValue(NumberOfLandsPlayedThisTurn, currentLandsPlayed + 1);
        }

        /// <summary>
        /// Returns whether it is currently possible to play a land.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public static bool CanPlayLand(Game game)
        {
            return m_bypassScope.InScope || GetNumLandsPlayedThisTurn(game) == 0;
        }

        private static int GetNumLandsPlayedThisTurn(Game game)
        {
            return game.TurnData.GetValue(NumberOfLandsPlayedThisTurn);
        }

        /// <summary>
        /// Set this to true to ignore the "one land per turn" rule. Use only in tests.
        /// </summary>
        internal static IDisposable Bypass()
        {
            return m_bypassScope.Begin();
        }

        #endregion
    }
}
