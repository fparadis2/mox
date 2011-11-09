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

using Mox.AI;

namespace Mox.Flow
{
    /// <summary>
    /// Manages the players input in a game.
    /// </summary>
    public interface IGameController
    {
        #region General

        /// <summary>
        /// Asks a question to the given player.
        /// </summary>
        /// <returns></returns>
        [ChoiceResolver(typeof(AI.Resolvers.AskModalChoiceResolver))]
        ModalChoiceResult AskModalChoice(MTGPart.Context context, Player player, ModalChoiceContext choiceContext);

        #endregion

        #region Main

        /// <summary>
        /// Gives the priority to the given <paramref name="player"/>.
        /// </summary>
        /// <returns>The action to do, null otherwise (to pass).</returns>
        [ChoiceResolver(typeof(AI.Resolvers.GivePriorityResolver))]
        Action GivePriority(MTGPart.Context context, Player player);

        /// <summary>
        /// Asks the given <paramref name="player"/> to pay for mana.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="player"></param>
        /// <param name="manaCost">The cost to pay.</param>
        /// <returns>The action to do (either a mana ability or a mana payment), null otherwise (to cancel).</returns>
        [ChoiceResolver(typeof(AI.Resolvers.PayManaResolver))]
        Action PayMana(MTGPart.Context context, Player player, ManaCost manaCost);

        /// <summary>
        /// Asks the player to choose a target from the possible targets.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="player"></param>
        /// <param name="targetInfo"></param>
        /// <returns></returns>
        [ChoiceResolver(typeof(AI.Resolvers.TargetResolver))]
        int Target(MTGPart.Context context, Player player, TargetContext targetInfo);

        /// <summary>
        /// Asks the player whether to mulligan.
        /// </summary>
        /// <returns>True to mulligan.</returns>
        [ChoiceResolver(typeof(AI.Resolvers.MulliganResolver))]
        bool Mulligan(MTGPart.Context context, Player player);

        #endregion

        #region Combat

        /// <summary>
        /// Asks the player to declare attackers.
        /// </summary>
        [ChoiceResolver(typeof(AI.Resolvers.DeclareAttackersResolver))]
        DeclareAttackersResult DeclareAttackers(MTGPart.Context context, Player player, DeclareAttackersContext attackInfo);

        /// <summary>
        /// Asks the player to declare blockers.
        /// </summary>
        [ChoiceResolver(typeof(AI.Resolvers.DeclareBlockersResolver))]
        DeclareBlockersResult DeclareBlockers(MTGPart.Context context, Player player, DeclareBlockersContext blockInfo);

        #endregion
    }
}
