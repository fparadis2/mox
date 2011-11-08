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

namespace Mox
{
    /// <summary>
    /// A cost that requires the controller to target an object.
    /// </summary>
    public class TargetCost : Cost
    {
        #region Variables

        private readonly Predicate<ITargetable> m_filter;
        private Resolvable<ITargetable> m_result;

        #endregion

        #region Constructor

        public TargetCost(Predicate<ITargetable> filter)
        {
            Throw.IfNull(filter, "filter");
            m_filter = filter;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Result of the target operation.
        /// </summary>
        public Resolvable<ITargetable> Result
        {
            get { return m_result; }
        }

        /// <summary>
        /// The filter to use for the target operation
        /// </summary>
        public Predicate<ITargetable> Filter
        {
            get { return m_filter; }
        }

        #endregion

        #region Overrides of Cost

        /// <summary>
        /// Returns false if the cost cannot be paid.
        /// </summary>
        /// <returns></returns>
        public override bool CanExecute(Game game, ExecutionEvaluationContext evaluationContext)
        {
            if (!evaluationContext.UserMode)
            {
                return true;
            }

            return EnumerateLegalTargets(game).Any();
        }

        /// <summary>
        /// Pays the cost. Returns false if the cost can't be paid.
        /// </summary>
        public override bool Execute(MTGPart.Context context, Player activePlayer)
        {
            m_result = new Resolvable<ITargetable>();

            List<int> possibleTargets = new List<int>(EnumerateLegalTargets(context.Game));

            if (possibleTargets.Count == 0)
            {
                return false;
            }

            TargetContext targetInfo = new TargetContext(true, possibleTargets.ToArray(), TargetContextType.Normal);

            ITargetable targetable;

            do
            {
                int result = context.Controller.Target(context, activePlayer, targetInfo);

                if (result == ObjectManager.InvalidIdentifier)
                {
                    return false;
                }

                targetable = Resolvable<ITargetable>.Resolve(context.Game, result);
            }
            while (!m_filter(targetable));

            Debug.Assert(possibleTargets.Contains(targetable.Identifier));

            m_result = new Resolvable<ITargetable>(targetable);

            Debug.Assert(!m_result.IsEmpty);
            Debug.Assert(m_filter(targetable));
            return true;
        }

        private IEnumerable<int> EnumerateLegalTargets(Game game)
        {
            return from targetable in GetAllTargetables(game)
                   where m_filter(targetable)
                   select targetable.Identifier;
        }

        private static IEnumerable<ITargetable> GetAllTargetables(Game game)
        {
            foreach (Player player in game.Players)
            {
                yield return player;
            }

            foreach (Card card in game.Zones.Battlefield.AllCards)
            {
                yield return card;
            }
        }

        #endregion

        #region Primitives

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

        private static bool IsTargetableCard(ITargetable targetable)
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

        #endregion
    }

    public class TargetCost<T> : TargetCost
        where T : ITargetable
    {
        #region Constructor

        public TargetCost(Predicate<ITargetable> filter)
            : base(filter)
        {
        }

        #endregion

        #region Operators

        public static TargetCost<T> operator &(TargetCost<T> a, TargetCost<T> b)
        {
            return new TargetCost<T>(x => a.Filter(x) && b.Filter(x));
        }

        public static TargetCost<T> operator |(TargetCost<T> a, TargetCost<T> b)
        {
            return new TargetCost<T>(x => a.Filter(x) || b.Filter(x));
        }

        #endregion
    }
}
