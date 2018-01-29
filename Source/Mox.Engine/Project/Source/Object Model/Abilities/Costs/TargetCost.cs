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
using System.Linq;
using Mox.Flow;

namespace Mox.Abilities
{
    /// <summary>
    /// A cost that requires the controller to target an object.
    /// </summary>
    public class TargetCost : Cost
    {
        #region Variables

        private readonly Filter m_filter;

        #endregion

        #region Constructor

        public TargetCost(Filter filter)
        {
            Throw.IfNull(filter, "filter");
            m_filter = filter;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The filter to use for the target operation
        /// </summary>
        public Filter Filter
        {
            get { return m_filter; }
        }

        #endregion

        #region Overrides of Cost

        public override bool CanExecute(Ability ability, AbilityEvaluationContext evaluationContext)
        {
            if (!evaluationContext.UserMode)
            {
                return true;
            }

            return EnumerateLegalTargets(evaluationContext.Game).Any();
        }

        public override void Execute(Part.Context context, Spell2 spell)
        {
            List<int> possibleTargets = new List<int>(EnumerateLegalTargets(context.Game));

            if (possibleTargets.Count == 0)
            {
                PushResult(context, false);
                return;
            }

            TargetContext targetInfo = new TargetContext(true, possibleTargets.ToArray(), TargetContextType.Normal);

            context.Schedule(new TargetPart(spell, this, targetInfo));
            return;
        }

        private IEnumerable<int> EnumerateLegalTargets(Game game)
        {
            List<GameObject> targets = new List<GameObject>();
            m_filter.EnumerateObjects(game, targets);

            return from targetable in targets
                   select targetable.Identifier;
        }

        #endregion

        #region Result

        /// <summary>
        /// Result of the target operation.
        /// </summary>
        public GameObject Resolve(Spell2 spell)
        {
            var result = ResolveImpl(spell);
            if (result.IsEmpty)
            {
                return null;
            }
            return result.Resolve(spell.Manager);
        }

        private Resolvable<GameObject> ResolveImpl(Spell2 spell)
        {
            return (Resolvable<GameObject>)spell.GetCostResult(this);
        }

        internal void SetResult(Spell2 spell, Resolvable<GameObject> result)
        {
            spell.SetCostResult(this, result);
        }

        #endregion

#warning todo spell_v2
        /*#region Primitives

        public static TargetCost<Player> Player()
        {
            return new TargetCost<Player>(t => t is Player);
        }

        public static TargetCost<Card> Card()
        {
            return new TargetCost<Card>(IsTargetableCard);
        }

        public static TargetCost<Card> Creature()
        {
            return new TargetCost<Card>(Card().OfAnyType(Type.Creature).Filter);
        }

        public static TargetCost<Card> Permanent()
        {
            return new TargetCost<Card>(Card().OfAnyType(CardExtensions.PermanentTypes).Filter);
        }

        private static bool IsTargetableCard(GameObject targetable)
        {
            Card card = targetable as Card;
            if (card != null)
            {
                // TODO: Wouldn't need to do this, already filtered by TargetCost.GetAllTargetables
                return card.Zone == card.Manager.Zones.Battlefield;
            }
            return false;
        }

        #endregion

        #region Operators

        public static TargetCost operator &(TargetCost a, TargetCost b)
        {
            return new TargetCost(x => a.Filter(x) && b.Filter(x));
        }

        public static TargetCost operator |(TargetCost a, TargetCost b)
        {
            return new TargetCost(x => a.Filter(x) || b.Filter(x));
        }

        #endregion*/

        #region Inner Types

        private class TargetPart : ChoicePart<TargetResult>
        {
            #region Variables

            private readonly Resolvable<Spell2> m_spell;
            private readonly TargetContext m_context;
            private readonly TargetCost m_parentCost;

            #endregion

            #region Constructor

            public TargetPart(Spell2 spell, TargetCost parentCost, TargetContext context)
                : base(spell.Controller)
            {
                m_spell = spell;
                m_context = context;
                m_parentCost = parentCost;
            }

            #endregion

            #region Overrides of ChoicePart<TargetResult>

            public override Choice GetChoice(Sequencer sequencer)
            {
                return new TargetChoice(ResolvablePlayer, m_context);
            }

            public override Part Execute(Context context, TargetResult choice)
            {
                if (!choice.IsValid)
                {
                    PushResult(context, false);
                    return null;
                }

                if (!m_context.IsValid(choice))
                {
                    return this;
                }

                var targetable = choice.Resolve<GameObject>(context.Game);
                Debug.Assert(targetable != null);

                var spell = m_spell.Resolve(context.Game);
                m_parentCost.SetResult(spell, new Resolvable<GameObject>(targetable));

                PushResult(context, true);
                return null;
            }

            #endregion
        }

        #endregion
    }
}
