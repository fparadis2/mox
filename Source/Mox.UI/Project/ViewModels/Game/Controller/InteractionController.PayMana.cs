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

namespace Mox.UI.Game
{
    partial class InteractionController
    {
        private class PayManaInteraction : PlayCardInteraction
        {
            #region Properties

            public ManaCost ManaCost
            {
                get;
                set;
            }

            private ManaPoolViewModel ManaPool
            {
                get { return PlayerViewModel.ManaPool; }
            }

            protected override EvaluationContextType EvaluationContextType
            {
                get
                {
                    return EvaluationContextType.ManaPayment;
                }
            }

            #endregion

            #region Methods

            public override bool Skip(out object result)
            {
                if (ManaPayment.TryGetTrivialPayment(ManaCost, PlayerViewModel.Source.ManaPool, out ManaPayment payment))
                {
                    result = new PayManaAction(payment);
                    return true;
                }

                return base.Skip(out result);
            }

            public override void Run()
            {
                base.Run();

                Model.Interaction.UserChoiceInteraction = UserChoiceInteractionModel.Cancel(string.Format("Pay {0}", ManaCost));
                Model.Interaction.UserChoiceSelected += Interaction_UserChoiceSelected;

                TagManaThatCanBePaid();
                ManaPool.ManaPaid += Interaction_ManaPaid;
            }

            protected override void End(object result)
            {
                Model.Interaction.UserChoiceSelected -= Interaction_UserChoiceSelected;
                ManaPool.ManaPaid -= Interaction_ManaPaid;

                base.End(result);
            }

            private void TagManaThatCanBePaid()
            {
                Debug.Assert(ManaCost.IsConcrete);

                var payingColors = ManaCost.GetPayingColors();
                var pool = ManaPool;

                TagManaThatCanBePaid(pool.White, payingColors, ManaColors.White);
                TagManaThatCanBePaid(pool.Blue, payingColors, ManaColors.Blue);
                TagManaThatCanBePaid(pool.Black, payingColors, ManaColors.Black);
                TagManaThatCanBePaid(pool.Red, payingColors, ManaColors.Red);
                TagManaThatCanBePaid(pool.Green, payingColors, ManaColors.Green);
                TagManaThatCanBePaid(pool.Colorless, payingColors, ManaColors.Colorless);
            }

            private void TagManaThatCanBePaid(ManaPoolViewModel.ManaPoolElementViewModel element, ManaColors payingColors, ManaColors color)
            {
                if (element.Amount > 0 && payingColors.HasFlag(color))
                    element.CanPay = true;
            }

            #endregion

            #region Event Handlers

            void Interaction_UserChoiceSelected(object sender, ItemEventArgs<UserChoiceModel> e)
            {
                Debug.Assert(e.Item.Type == UserChoiceType.Cancel);
                End(null);
            }

            void Interaction_ManaPaid(object sender, ItemEventArgs<Color> e)
            {
                ManaAmount amount = new ManaAmount();
                amount.Add(e.Item, 1);

                if (!ManaPayment.TryGetTrivialPayment(ManaCost, amount, out ManaPayment payment))
                    throw new NotSupportedException("Todo: revisit ambiguous mana payment");

                End(new PayManaAction(payment));
            }

            #endregion
        }
    }
}
