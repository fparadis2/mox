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

namespace Mox.Flow
{
    /// <summary>
    /// A controller that always does the default action (passing for example)
    /// </summary>
    public class DeadGameController : IGameController
    {
        #region Implementation of IGameController

        public ModalChoiceResult AskModalChoice(Part<IGameController>.Context context, Player player, ModalChoiceContext choiceContext)
        {
            return choiceContext.DefaultChoice;
        }

        /// <summary>
        /// Gives the priority to the given <paramref name="player"/>.
        /// </summary>
        /// <returns>The action to do, null otherwise (to pass).</returns>
        public Action GivePriority(Part<IGameController>.Context context, Player player)
        {
            return null;
        }

        /// <summary>
        /// Asks the given <paramref name="player"/> to pay for mana.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="player"></param>
        /// <param name="manaCost">The cost to pay.</param>
        /// <returns>The action to do (either a mana ability or a mana payment), null otherwise (to cancel).</returns>
        public Action PayMana(Part<IGameController>.Context context, Player player, ManaCost manaCost)
        {
            return null;
        }

        /// <summary>
        /// Asks the player to choose a target from the possible targets.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="player"></param>
        /// <param name="targetInfo"></param>
        /// <returns></returns>
        public int Target(Part<IGameController>.Context context, Player player, TargetContext targetInfo)
        {
            return targetInfo.AllowCancel ? ObjectManager.InvalidIdentifier : targetInfo.Targets.First();
        }

        /// <summary>
        /// Asks the player whether to mulligan.
        /// </summary>
        /// <returns>True to mulligan.</returns>
        public bool Mulligan(Part<IGameController>.Context context, Player player)
        {
            return false;
        }

        public DeclareAttackersResult DeclareAttackers(Part<IGameController>.Context context, Player player, DeclareAttackersContext attackInfo)
        {
            return DeclareAttackersResult.Empty;
        }

        public DeclareBlockersResult DeclareBlockers(Part<IGameController>.Context context, Player player, DeclareBlockersContext blockInfo)
        {
            return DeclareBlockersResult.Empty;
        }

        #endregion
    }
}
