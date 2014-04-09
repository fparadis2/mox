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
    /// A cost that requires to tap/untap a card.
    /// </summary>
    public class TapCost : Cost
    {
        #region Variables

        private readonly Resolvable<Card> m_card;
        private readonly bool m_tap;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public TapCost(Card card, bool tap)
        {
            Throw.IfNull(card, "card");

            m_card = card;
            m_tap = tap;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The card to tap/untap.
        /// </summary>
        public Resolvable<Card> Card
        {
            get { return m_card; }
        }

        /// <summary>
        /// Whether to tap or untap (true for tap).
        /// </summary>
        public bool DoTap
        {
            get { return m_tap; }
        }

        #endregion

        #region Methods

        public override bool CanExecute(Game game, ExecutionEvaluationContext evaluationContext)
        {
            var card = m_card.Resolve(game);
            return CanExecuteImpl(card);
        }

        /// <summary>
        /// Taps/Untaps the card.
        /// </summary>
        /// <returns></returns>
        public override void Execute(Part.Context context, Player activePlayer)
        {
            var card = m_card.Resolve(context.Game);
            if (!CanExecuteImpl(card))
            {
                PushResult(context, false);
                return;
            }

            Debug.Assert(card.Tapped != DoTap);
            card.Tapped = DoTap;
            PushResult(context, true);
        }

        private bool CanExecuteImpl(Card card)
        {
            return card.Tapped != DoTap && !card.HasSummoningSickness;
        }

        #endregion
    }
}
