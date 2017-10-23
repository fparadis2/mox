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

namespace Mox
{
    /// <summary>
    /// A cost that requires that the player pays some mana.
    /// </summary>
    public class PayManaCost : Cost
    {
        #region Parts

        private class PayManaPart : ChoicePart<Action>
        {
            #region Variables

            private readonly ManaCost m_manaCost;

            #endregion

            #region Constructor

            public PayManaPart(Player player, ManaCost manaCost)
                : base(player)
            {
                Throw.IfNull(player, "player");
                Throw.InvalidArgumentIf(manaCost.IsEmpty, "Empty mana cost", "manaCost");

                m_manaCost = manaCost;
            }

            #endregion

            #region Overrides of ChoicePart<Action>

            public override Choice GetChoice(Sequencer sequencer)
            {
                return new PayManaChoice(ResolvablePlayer, m_manaCost);
            }

            public override Part Execute(Context context, Action action)
            {
                if (action == null)
                {
                    PushResult(context, false);
                    return null;
                }

                var player = GetPlayer(context);
                ExecutionEvaluationContext evaluationContext = new ExecutionEvaluationContext { Type = EvaluationContextType.ManaPayment };
                if (!action.CanExecute(player, evaluationContext))
                {
                    // retry
                    return this;
                }

                ManaCost remainingCost = m_manaCost;

                if (action is PayManaAction)
                {
                    PayManaAction payManaAction = (PayManaAction)action;
                    using (context.Game.Controller.BeginCommandGroup())
                    {
                        remainingCost = PayMana(player, remainingCost, payManaAction.Payment);
                    }
                }
                else
                {
                    action.Execute(context, player);
                }


                if (remainingCost.IsEmpty)
                {
                    PushResult(context, true);
                    return null;
                }

                return new PayManaPart(player, remainingCost);
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly ManaCost m_manaCost;

        #endregion

        #region Constructor

        public PayManaCost(ManaCost manaCost)
        {
            m_manaCost = manaCost;
        }

        #endregion

        #region Properties

        public ManaCost ManaCost
        {
            get { return m_manaCost; }
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
                return true; // AI will always try to play it

            if (ManaCost == null || ManaCost.IsEmpty)
                return true;

            // todo
            //PayManaCostEvaluator evaluator = PayManaCostEvaluator.Create(player, ManaCost);
            //return evaluator.CanPotentiallyPay();
            return true;
        }

        /// <summary>
        /// Pays the cost. Returns false if the cost can't be paid.
        /// </summary>
        public override void Execute(Part.Context context, Player player)
        {
            ManaCost cost = ManaCost;

            if (cost == null || cost.IsEmpty)
            {
                PushResult(context, true);
            }
            else
            {
                context.Schedule(new PayManaPart(player, cost));
            }
        }

        private static ManaCost PayMana(Player player, ManaCost cost, ManaPayment payment)
        {
            Throw.InvalidOperationIf(!cost.IsConcrete, "TODO");

            List<Color> paymentColors = new List<Color>(payment.Payments);

            // Pay colored mana first
            foreach (ManaSymbol symbol in cost.Symbols)
            {
                Debug.Assert(!ManaSymbolHelper.IsHybrid(symbol));
                Debug.Assert(symbol != ManaSymbol.X);
                Debug.Assert(symbol != ManaSymbol.Y);
                Debug.Assert(symbol != ManaSymbol.Z);
                Debug.Assert(symbol != ManaSymbol.S, "TODO");

                Color payColor = ManaSymbolHelper.GetColor(symbol);
                if (paymentColors.Remove(payColor) && player.ManaPool[payColor] > 0)
                {
                    // Ok, this can be paid.
                    player.ManaPool[payColor] -= 1;
                    cost = cost.Remove(symbol);
                }
            }

            // Pay colorless mana with colorless mana
            int numColorlessMana = paymentColors.RemoveAll(color => color == Color.None);
            numColorlessMana = Math.Min(cost.Colorless, numColorlessMana); // Cannot pay more than the cost requires
            numColorlessMana = Math.Min(player.ManaPool[Color.None], numColorlessMana); // Cannot pay more than available in mana pool
            player.ManaPool[Color.None] -= numColorlessMana;
            cost = cost.RemoveColorless(numColorlessMana);

            // Pay colorless mana with colored mana if available
            foreach (Color color in paymentColors)
            {
                if (cost.Colorless > 0 && player.ManaPool[color] > 0)
                {
                    player.ManaPool[color] -= 1;
                    cost = cost.RemoveColorless(1);
                }
            }

            return cost;
        }

        #endregion
    }
}
