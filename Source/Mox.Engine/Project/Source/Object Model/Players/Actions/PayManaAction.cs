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

using Mox.Abilities;

namespace Mox
{
    [Serializable]
    public class PayManaAction : PlayerAction
    {
        #region Variables

        private readonly ManaPayment m_payment;

        #endregion

        #region Constructor

        public PayManaAction(ManaPayment payment)
        {
            m_payment = payment;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The payment.
        /// </summary>
        public ManaPayment Payment
        {
            get { return m_payment; }
        }

        #endregion

        #region Overrides of Action

        /// <summary>
        /// Returns true if the action can be executed.
        /// </summary>
        public override bool CanExecute(AbilityEvaluationContext evaluationContext)
        {
            return evaluationContext.Type == AbilityEvaluationContextType.ManaPayment;
        }

        /// <summary>
        /// Executes the action.
        /// </summary>
        public override void Execute(Part.Context context, Player player)
        {
            throw new InvalidProgramException("This action is not meant to be executed!");
        }

        public override string ToString()
        {
            return m_payment.ToString();
        }

        #endregion
    }
}
