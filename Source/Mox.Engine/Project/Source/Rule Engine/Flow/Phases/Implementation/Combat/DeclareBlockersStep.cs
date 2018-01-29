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

using Mox.Abilities;

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

            public override Choice GetChoice(Sequencer sequencer)
            {
                return new DeclareBlockersChoice(ResolvablePlayer, m_context);
            }

            public override Part Execute(Context context, DeclareBlockersResult result)
            {
                Debug.Assert(!context.Game.CombatData.Attackers.IsEmpty);
                
                if (!ValidateBlock(m_context, result, context))
                {
                    // retry if not valid.
                    return this;
                }

                if (!CheckRestrictionsAndRequirements(context, result))
                {
                    // retry if not valid.
                    return this;
                }

                // todo Pay needed costs
                // context.Schedule(new BeginTransactionPart(PayBlockingCosts.TransactionToken));
                return new AssignBlockingCreatures(GetPlayer(context), result);
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

            private bool CheckRestrictionsAndRequirements(Context context, DeclareBlockersResult result)
            {
                AbilityEvaluationContext evaluationContext = new AbilityEvaluationContext(GetPlayer(context), AbilityEvaluationContextType.Block);

                foreach (var ability in GetBlockAbilities(context, result))
                {
                    if (!ability.CanPlay(evaluationContext))
                        return false;

                    Debug.Assert(ability.SpellDefinition.Costs.Count == 0, "TODO: Handle real attack abilities");
                    Debug.Assert(ability.SpellDefinition.Actions.Count == 0, "TODO: Handle real attack abilities");
                }

                return true;
            }

            private IEnumerable<Ability> GetBlockAbilities(Context context, DeclareBlockersResult result)
            {
                foreach (Card attacker in result.GetBlockers(context.Game))
                {
                    var abilities = attacker.Abilities.OfType<SpellAbility>().Where(ability => ability.AbilityType == AbilityType.Block);

                    foreach (var ability in abilities)
                    {
                        yield return ability;
                    }
                }
            }

            public static Part Create(CombatData combatData)
            {
                Player defendingPlayer = combatData.DefendingPlayer;
                Debug.Assert(defendingPlayer != null);

                DeclareBlockersContext blockInfo = DeclareBlockersContext.ForPlayer(defendingPlayer);

                if (!blockInfo.IsEmpty)
                {
                    return new DeclareBlockersImpl(defendingPlayer, blockInfo);
                }

                return null;
            }
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

            public override Part Execute(Context context)
            {
                Player player = GetPlayer(context);
                //bool result = context.PopArgument<bool>(PayBlockingCosts.ArgumentToken);
                bool result = true;

                if (result)
                {
                    // Every creature still controlled by player becomes a blocking creature
                    DeclareBlockersResult blockers = GetValidBlockers(m_result, context, player);
                    context.Game.CombatData.Blockers = blockers;

                    // TODO: Handle damage assignment order

                    return null;
                }

                // Retry
                return DeclareBlockersImpl.Create(context.Game.CombatData);
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

        protected override Part SequenceImpl(Part.Context context, Player player)
        {
            if (!context.Game.CombatData.Attackers.IsEmpty)
            {
                var declareBlockers = DeclareBlockersImpl.Create(context.Game.CombatData);

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
