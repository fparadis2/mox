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
using Mox.Flow;

namespace Mox
{
    /// <summary>
    /// The controller of a player (human, AI, network client, etc...)
    /// </summary>
    public interface IClientInput
    {
        #region Methods

        #region General

        /// <summary>
        /// Asks a question to the given player.
        /// </summary>
        /// <returns></returns>
        ModalChoiceResult AskModalChoice(ModalChoiceContext context);

        #endregion

        #region Main

        /// <summary>
        /// Gives the priority to this controller.
        /// </summary>
        /// <returns>The action to do, null otherwise (to pass).</returns>
        Action GivePriority();

        /// <summary>
        /// Asks the player to pay for mana.
        /// </summary>
        /// <param name="manaCost">The cost to pay.</param>
        /// <returns>The action to do (either a mana ability or a mana payment), null otherwise (to cancel).</returns>
        Action PayMana(ManaCost manaCost);

        /// <summary>
        /// Asks the player whether to mulligan.
        /// </summary>
        /// <returns>True to mulligan.</returns>
        bool Mulligan();

        /// <summary>
        /// Asks the player to choose a target.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        int Target(TargetContext context);

        #endregion

        #region Combat

        /// <summary>
        /// Asks the player to declare attackers.
        /// </summary>
        DeclareAttackersResult DeclareAttackers(DeclareAttackersContext attackInfo);

        /// <summary>
        /// Asks the player to declare blockers.
        /// </summary>
        DeclareBlockersResult DeclareBlockers(DeclareBlockersContext blockInfo);

        #endregion

        #endregion
    }
}
