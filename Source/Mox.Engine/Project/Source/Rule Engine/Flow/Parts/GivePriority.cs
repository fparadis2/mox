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

namespace Mox.Flow.Parts
{
    /// <summary>
    /// A part that gives priority to a player.
    /// </summary>
    public class GivePriority : MTGPart
    {
        #region Argument Token

        internal static readonly object ArgumentToken = "GivePriority";

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="player">Player to give priority to.</param>
        public GivePriority(Player player)
            : base(player)
        {
        }

        #endregion

        #region Overrides of Part

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
            Action action = context.Controller.GivePriority(context, player);

            if (action != null)
            {
                ExecutionEvaluationContext evaluationContext = new ExecutionEvaluationContext { Type = EvaluationContextType.Normal };

                if (!action.CanExecute(player, evaluationContext))
                {
                    // Retry
                    return this;
                }

                action.Execute(context, player);
            }

            context.PushArgument(action == null, ArgumentToken);
            return null;
        }

        #endregion
    }
}
