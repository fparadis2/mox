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

namespace Mox.Flow.Phases
{
    public class DeclareBlockersStep : Step
    {
        #region Inner Parts

        private class DeclareBlockersImpl : ChoicePart<DeclareBlockersResult>
        {
            private readonly DeclareBlockersContext m_context;

            public DeclareBlockersImpl(Player player, DeclareBlockersContext context)
                : base(player)
            {
                m_context = context;
            }

            public override Choice GetChoice(Context context)
            {
                return new DeclareBlockersChoice(ResolvablePlayer, m_context);
            }

            public override NewPart Execute(Context context, DeclareBlockersResult result)
            {
                Debug.Assert(!context.Game.CombatData.Attackers.IsEmpty);
                
                if (!ValidateBlock(m_context, result, context))
                {
                    // retry if not valid.
                    return this;
                }

                // Pay needed costs
                context.Schedule(new BeginTransactionPart(PayBlockingCosts.TransactionToken));
                return new PayBlockingCosts(GetPlayer(context), result);
            }

            private static bool ValidateBlock(DeclareBlockersContext blockInfo, DeclareBlockersResult result, Context context)
            {
                if (!blockInfo.IsValid(result))
                {
                    return false;
                }

                // Check evasion abilities
                foreach (KeyValuePair<Card, Card> blockerPair in result.GetBlockerPairs(context.Game))
                {
                    foreach (EvasionAbility evasionAbility in blockerPair.Key.Abilities.OfType<EvasionAbility>())
                    {
                        if (!evasionAbility.CanBlock(blockerPair.Key, blockerPair.Value))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            public static NewPart Create(Player player)
            {
                // TODO: Support more than two players.
                Player defendingPlayer = Player.GetNextPlayer(player);
                DeclareBlockersContext blockInfo = DeclareBlockersContext.ForPlayer(defendingPlayer);

                if (!blockInfo.IsEmpty)
                {
                    return new DeclareBlockersImpl(defendingPlayer, blockInfo);
                }

                return null;
            }
        }

        private class PayBlockingCosts : PayAttackingCosts
        {
            #region Variables

            private readonly DeclareBlockersResult m_result;

            #endregion

            #region Ctor

            public PayBlockingCosts(Player player, DeclareBlockersResult result)
                : base(player)
            {
                Debug.Assert(result != null);
                m_result = result;
            }

            #endregion

            #region Implementation

            protected override AbilityType Type
            {
                get { return AbilityType.Block; }
            }

            protected override EvaluationContextType EvaluationType
            {
                get { return EvaluationContextType.Block; }
            }

            protected override NewPart CreateNextPart(Context context)
            {
                return new AssignBlockingCreatures(GetPlayer(context), m_result);
            }

            protected override IEnumerable<Card> GetInvolvedCards(Context context)
            {
                return m_result.GetBlockers(context.Game);
            }

            #endregion
        }

        private class AssignBlockingCreatures : PlayerPart
        {
            private readonly DeclareBlockersResult m_result;

            public AssignBlockingCreatures(Player player, DeclareBlockersResult result)
                : base(player)
            {
                Debug.Assert(result != null);
                m_result = result;
            }

            public override NewPart Execute(Context context)
            {
                Player player = GetPlayer(context);
                bool result = context.PopArgument<bool>(PayBlockingCosts.ArgumentToken);

                if (result)
                {
                    // Every creature still controlled by player becomes a blocking creature
                    DeclareBlockersResult blockers = GetValidBlockers(m_result, context, player);
                    context.Game.CombatData.Blockers = blockers;

                    // TODO: Handle damage assignment order

                    return null;
                }

                // Retry
                return DeclareBlockersImpl.Create(context.Game.State.ActivePlayer);
            }

            private static DeclareBlockersResult GetValidBlockers(DeclareBlockersResult result, Context context, Player player)
            {
                var legalBlockingCreatures = result.GetBlockers(context.Game).Where(c => c.Controller == player).Select(c => c.Identifier);
                var blockers = result.Blockers.Where(pair => legalBlockingCreatures.Contains(pair.BlockingCreatureId));
                return new DeclareBlockersResult(blockers.ToArray());
            }
        }

        #endregion

        #region Constructor

        public DeclareBlockersStep()
            : base(Steps.DeclareBlockers)
        {
        }

        #endregion

        #region Methods

        protected override NewPart SequenceImpl(NewPart.Context context, Player player)
        {
            if (!context.Game.CombatData.Attackers.IsEmpty)
            {
                var declareBlockers = DeclareBlockersImpl.Create(player);

                if (declareBlockers != null)
                {
                    context.Schedule(declareBlockers);
                }
                return base.SequenceImpl(context, player);
            }

            return null;
        }

        #endregion
    }
}
