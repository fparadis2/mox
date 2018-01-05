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
using System.Diagnostics;
using Mox.Flow;

namespace Mox
{
    /// <summary>
    /// A cost for an action.
    /// </summary>
    public abstract class Cost
    {
        #region ArgumentToken

        internal static readonly object ArgumentToken = "Cost";

        #endregion

        #region Inner Types

        /// <summary>
        /// The result of the evaluation of a cost.
        /// </summary>
        public class EvaluationResult
        {
            public bool IsSuccessful
            {
                get;
                set;
            }

            public static implicit operator bool(EvaluationResult result)
            {
                return result.IsSuccessful;
            }

            public static implicit operator EvaluationResult(bool isSuccessful)
            {
                return new EvaluationResult { IsSuccessful = isSuccessful };
            }
        }

        private class CannotPlayCost : Cost
        {
            #region Overrides of Cost

            public override bool CanExecute(Game game, ExecutionEvaluationContext evaluationContext)
            {
                return false;
            }

            public override void Execute(Part.Context context, Player activePlayer)
            {
                throw new InvalidOperationException("Not supposed to ever execute this cost");
            }

            #endregion
        }

        #endregion

        #region Methods

        protected internal static void PushResult(Part.Context context, bool result)
        {
            context.PushArgument(result, ArgumentToken);
        }

        protected internal static bool PopResult(Part.Context context)
        {
            return context.PopArgument<bool>(ArgumentToken);
        }

        /// <summary>
        /// Pays the cost. Pushes the result on the argument stack.
        /// </summary>
        public abstract void Execute(Part.Context context, Player activePlayer);

        /// <summary>
        /// Returns false if the cost cannot be paid.
        /// </summary>
        /// <returns></returns>
        public abstract bool CanExecute(Game game, ExecutionEvaluationContext evaluationContext);

        #endregion

        #region Static costs

        /// <summary>
        /// A cost that can never be "paid".
        /// </summary>
        public static Cost CannotPlay
        {
            get { return new CannotPlayCost(); }
        }

        /// <summary>
        /// A cost that requires the source to be tapped.
        /// </summary>
        public static TapCost Tap(Card card)
        {
            return new TapCost(card, true);
        }

        #endregion
    }
}
