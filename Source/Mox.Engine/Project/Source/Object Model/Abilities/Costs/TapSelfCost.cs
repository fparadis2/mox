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

using Mox.Flow;

namespace Mox.Abilities
{
    /// <summary>
    /// A cost that requires to tap/untap the source.
    /// </summary>
    public class TapSelfCost : Cost
    {
        #region Variables

        private readonly bool m_tap;

        #endregion

        #region Constructor

        public TapSelfCost(bool tap)
        {
            m_tap = tap;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Whether to tap or untap (true for tap).
        /// </summary>
        public bool DoTap
        {
            get { return m_tap; }
        }

        #endregion

        #region Methods

        public override bool CanExecute(Ability ability, AbilityEvaluationContext evaluationContext)
        {
            return CanExecuteImpl(ability.Source);
        }

        /// <summary>
        /// Taps/Untaps the card.
        /// </summary>
        /// <returns></returns>
        public override void Execute(Part.Context context, Spell2 spell)
        {
            var source = spell.Source;

            if (!CanExecuteImpl(source))
            {
                PushResult(context, false);
                return;
            }

            Debug.Assert(source.Tapped != DoTap);
            source.Tapped = DoTap;
            PushResult(context, true);
        }

        private bool CanExecuteImpl(Card card)
        {
            return card.Tapped != DoTap && !card.HasSummoningSickness;
        }

        #endregion
    }
}
