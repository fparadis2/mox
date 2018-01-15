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
    /// A cost that requires to tap/untap a card.
    /// </summary>
    public class TapCost : Cost
    {
        #region Variables

        private readonly ObjectResolver m_card;
        private readonly bool m_tap;

        #endregion

        #region Constructor

#warning todo spell_v2 needed?
        public TapCost(Card card, bool tap)
        {
            Throw.IfNull(card, "card");
            m_card = card;
            m_tap = tap;
        }

        public TapCost(ObjectResolver card, bool tap)
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
        public ObjectResolver Card
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

        public override bool CanExecute(AbilityEvaluationContext evaluationContext, SpellContext spellContext)
        {
            foreach (var card in m_card.Resolve<Card>(evaluationContext.Game, spellContext))
            {
                if (!CanExecuteImpl(card))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Taps/Untaps the card.
        /// </summary>
        /// <returns></returns>
        public override void Execute(Part.Context context, SpellContext spellContext)
        {
            List<Card> cards = new List<Mox.Card>();

            foreach (var card in m_card.Resolve<Card>(context.Game, spellContext))
            {
                if (!CanExecuteImpl(card))
                {
                    PushResult(context, false);
                    return;
                }

                cards.Add(card);
            }

            foreach (var card in cards)
            {
                Debug.Assert(card.Tapped != DoTap);
                card.Tapped = DoTap;
            }

            PushResult(context, true);
        }

        private bool CanExecuteImpl(Card card)
        {
            return card.Tapped != DoTap && !card.HasSummoningSickness;
        }

        #endregion
    }
}
