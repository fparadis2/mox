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

namespace Mox.Abilities
{
    public class SacrificeCost : Cost
    {
        public SacrificeCost(ObjectResolver cards)
        {
            Throw.IfNull(cards, "cards");
            Cards = cards;
        }

        public ObjectResolver Cards { get; }

        public override CostOrder Order => CostOrder.Sacrifice;

        public override bool CanExecute(Ability ability, AbilityEvaluationContext evaluationContext)
        {
            return true;
        }

        public override void Execute(Part.Context context, Spell2 spell)
        {
            foreach (var card in Cards.Resolve<Card>(spell))
            {
                if (card.Zone.ZoneId != Zone.Id.Battlefield ||
                    card.Controller != spell.Controller)
                {
                    PushResult(context, false);
                    return;
                }
            }

            foreach (var card in Cards.Resolve<Card>(spell))
            {
                card.Sacrifice();
            }

            PushResult(context, true);
        }
    }
}
