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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mox.Flow.Phases
{
    public class DeclareAttackersStep : Step
    {
        #region Inner Parts

        private class DeclareAttackersImpl : ChoicePart<DeclareAttackersResult>
        {
            private readonly DeclareAttackersContext m_context;

            public DeclareAttackersImpl(Player player, DeclareAttackersContext context)
                : base(player)
            {
                Debug.Assert(!context.IsEmpty);
                m_context = context;
            }

            public override Choice GetChoice(Game game)
            {
                return new DeclareAttackersChoice(ResolvablePlayer, m_context);
            }

            public override NewPart Execute(Context context, DeclareAttackersResult result)
            {
                if (!m_context.IsValid(result))
                {
                    // retry if not valid.
                    return this;
                }

                // Check restrictions & requirements (TODO)

                // Tap chosen creatures & pay needed costs
                context.Schedule(new BeginTransactionPart(PayAttackingCosts.TransactionToken));
                context.Schedule(new TapAttackingCreatures(GetPlayer(context), result));
                return null;
            }
        }

        private class TapAttackingCreatures : PlayerPart
        {
            #region Variables

            private readonly DeclareAttackersResult m_result;

            #endregion

            #region Ctor

            public TapAttackingCreatures(Player player, DeclareAttackersResult result)
                : base(player)
            {
                m_result = result;
            }

            #endregion

            #region Methods

            public override NewPart Execute(Context context)
            {
                foreach (Card attackingCreature in m_result.GetAttackers(context.Game))
                {
                    if (!attackingCreature.HasAbility<VigilanceAbility>())
                    {
                        Debug.Assert(!attackingCreature.Tapped);
                        attackingCreature.Tapped = true;
                    }
                }

                return new PayAttackingCostsImpl(GetPlayer(context), m_result);
            }

            #endregion
        }

        private class PayAttackingCostsImpl : PayAttackingCosts
        {
            #region Variables

            private readonly DeclareAttackersResult m_result;

            #endregion

            #region Ctor

            public PayAttackingCostsImpl(Player player, DeclareAttackersResult result)
                : base(player)
            {
                Debug.Assert(result != null);
                m_result = result;
            }

            #endregion

            #region Implementation

            protected override AbilityType Type
            {
                get { return AbilityType.Attack; }
            }

            protected override EvaluationContextType EvaluationType
            {
                get { return EvaluationContextType.Attack; }
            }

            protected override NewPart CreateNextPart(Context context)
            {
                return new AssignAttackingCreatures(GetPlayer(context), m_result);
            }

            protected override IEnumerable<Card> GetInvolvedCards(Context context)
            {
                return m_result.GetAttackers(context.Game);
            }

            #endregion
        }

        private class AssignAttackingCreatures : PlayerPart
        {
            private readonly DeclareAttackersResult m_result;

            public AssignAttackingCreatures(Player player, DeclareAttackersResult result)
                : base(player)
            {
                Debug.Assert(result != null);
                m_result = result;
            }

            public override NewPart Execute(Context context)
            {
                Player player = GetPlayer(context);
                bool result = context.PopArgument<bool>(PayAttackingCosts.ArgumentToken);

                if (result)
                {
                    // Every creature still controlled by player becomes an attacking creature
                    DeclareAttackersResult legalAttackers = GetValidAttackers(m_result, context, player);
                    context.Game.CombatData.Attackers = legalAttackers;

                    return null;
                }
                
                // Retry
                return new DeclareAttackersImpl(player, DeclareAttackersContext.ForPlayer(player));
            }

            private static DeclareAttackersResult GetValidAttackers(DeclareAttackersResult result, Context context, Player player)
            {
                var validCreatures = result.GetAttackers(context.Game).Where(c => c.Controller == player);
                return new DeclareAttackersResult(validCreatures.ToArray());
            }
        }

        #endregion

        #region Constructor

        public DeclareAttackersStep()
            : base(Steps.DeclareAttackers)
        {
        }

        #endregion

        #region Methods

        protected override NewPart SequenceImpl(NewPart.Context context, Player player)
        {
            DeclareAttackersContext attackInfo = DeclareAttackersContext.ForPlayer(player);

            if (!attackInfo.IsEmpty)
            {
                context.Schedule(new DeclareAttackersImpl(player, attackInfo));
            }
            return base.SequenceImpl(context, player);
        }

        #endregion
    }
}
