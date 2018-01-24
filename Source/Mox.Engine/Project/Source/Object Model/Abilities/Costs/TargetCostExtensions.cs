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
using System.Diagnostics;
using System.Linq;

namespace Mox.Abilities
{
#warning todo spell_v2
    /*public static class TargetCostExtensions
    {
        #region Players

        public static TargetCost<Player> Opponent(this TargetCost<Player> cost, Player player)
        {
            return cost & new TargetCost<Player>(targetable => targetable.Identifier != player.Identifier);
        }

        #endregion

        #region Cards

        public static TargetCost<Card> OfAnyType(this TargetCost<Card> cost, Type types)
        {
            return cost & new TargetCost<Card>(targetable => ((Card)targetable).IsAny(types));
        }

        public static TargetCost<Card> OfAnySubType(this TargetCost<Card> cost, params SubType[] types)
        {
            return cost & new TargetCost<Card>(targetable => ((Card)targetable).IsAny(types));
        }

        public static TargetCost<Card> OfAnyColor(this TargetCost<Card> cost, Color colors)
        {
            return cost & new TargetCost<Card>(targetable => ((Card)targetable).IsAny(colors));
        }

        public static TargetCost<Card> UnderControl(this TargetCost<Card> cost, Player controller)
        {
            return cost & new TargetCost<Card>(targetable => ((Card)targetable).Controller.Identifier == controller.Identifier);
        }

        public static TargetCost<Card> With<TAbility>(this TargetCost<Card> cost)
            where TAbility : Ability
        {
            return cost & new TargetCost<Card>(targetable => ((Card)targetable).HasAbility<TAbility>());
        }

        public static TargetCost<Card> Without<TAbility>(this TargetCost<Card> cost)
            where TAbility : Ability
        {
            return cost & new TargetCost<Card>(targetable => !((Card)targetable).HasAbility<TAbility>());
        }

        public static TargetCost<Card> Attacking(this TargetCost<Card> cost)
        {
            return cost & new TargetCost<Card>(targetable =>
            {
                Card card = (Card)targetable;
                return card.Manager.CombatData.Attackers.AttackerIdentifiers.Contains(card.Identifier);
            });
        }

        public static TargetCost<Card> Blocking(this TargetCost<Card> cost)
        {
            return cost & new TargetCost<Card>(targetable =>
            {
                Card card = (Card)targetable;
                return card.Manager.CombatData.Blockers.Blockers.Any(pair => pair.BlockingCreatureId == card.Identifier);
            });
        }

        public static TargetCost<Card> Tapped(this TargetCost<Card> cost)
        {
            return cost & new TargetCost<Card>(targetable => ((Card) targetable).Tapped);
        }

        public static TargetCost<Card> Untapped(this TargetCost<Card> cost)
        {
            return cost & new TargetCost<Card>(targetable => !((Card)targetable).Tapped);
        }

        public static TargetCost Except(this TargetCost cost, TargetCost result)
        {
            return cost & new TargetCost(targetable => targetable.Identifier != result.ResolveIdentifier((Game)targetable.Manager));
        }

        public static TargetCost ExceptThisResult(this TargetCost cost)
        {
            return cost.Except(cost);
        }

        #endregion

        #region Derived costs

        public static TargetSacrificeCost Sacrifice(this TargetCost<Card> cost)
        {
            return new TargetSacrificeCost(cost.Filter);
        }

        #endregion
    }*/
}