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

namespace Mox.AI
{
    /// <summary>
    /// My own implementation of an AI :)
    /// Feel free to experiment with your own!
    /// </summary>
    public class DefaultMinMaxAlgorithm : BaseMinMaxAlgorithm
    {
        #region Constants

        private const int LifeModifier = 100; // Life is pretty important in magic :)
        private const int BattlefieldSizeModifier = 5; // per card in the battlefield

        #endregion

        #region Variables

        #endregion

        #region Constructor

        public DefaultMinMaxAlgorithm(Player maximizingPlayer, AIParameters parameters)
            : base(maximizingPlayer, parameters)
        {
        }

        #endregion

        #region Overrides of BaseMinMaxAlgorithm

        /// <summary>
        /// Returns true if the current search should be stopped at this depth.
        /// </summary>
        /// <remarks>
        /// Normally this would return true if the game has ended, or if the search reached a specific depth.
        /// </remarks>
        /// <param name="tree"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public override bool IsTerminal(IMinimaxTree tree, Game game)
        {
            if (game.State.HasEnded)
            {
                return true;
            }

            if (!CanEndSearch(game))
            {
                return false;
            }

            return tree.Depth >= Parameters.MinimumTreeDepth;
        }

        private static bool CanEndSearch(Game game)
        {
            // Covered by IUninterruptiblePart
            /*if (!game.SpellStack.IsEmpty)
            {
                return false;
            }*/

            switch (game.State.CurrentStep)
            {
                case Steps.BeginningOfCombat:
                case Steps.DeclareAttackers:
                case Steps.DeclareBlockers:
                case Steps.CombatDamage:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Computes the "value" of the current "game state".
        /// </summary>
        /// <returns></returns>
        public override float ComputeHeuristic(Game game, bool considerGameEndingState)
        {
            // Check for end conditions
            if (game.State.HasEnded && considerGameEndingState)
            {
                return IsMaximizingPlayer(game.State.Winner) ? MaxValue : MinValue;
            }

            float value = 0;

            foreach (Player player in game.Players)
            {
                int sign = IsMaximizingPlayer(player) ? +1 : -1;

                // Life
                value += ComputeLifeValue(player.Life) * sign;

                // Cards in play are usually good
                foreach (Card card in player.Battlefield)
                {
                    value += ComputeCardValue(card) * sign;
                }
            }

            Debug.Assert(value >= MinValue && value <= MaxValue, "Overflow");
            return value;
        }

        private static float ComputeLifeValue(int life)
        {
            return (float)Math.Log(Math.Max(0, life) + 0.5f) * LifeModifier;
        }

        private static float ComputeCardValue(Card card)
        {
            int baseValue = 2;

            if (card.Is(Type.Creature))
            {
                baseValue = (card.Toughness + card.Power);
            }

            return baseValue * BattlefieldSizeModifier;
        }

        #endregion
    }
}
