﻿// Copyright (c) François Paradis
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
using System.Text;
using System.Threading;

namespace Mox.UI
{
    /// <summary>
    /// Translates <see cref="IClientController"/> actions into <see cref="InteractionModel"/> terms.
    /// </summary>
    partial class InteractionController
    {
        #region Inner Types

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

            public override void Run()
            {
                base.Run();

                Model.Interaction.UserChoiceInteraction = UserChoiceInteractionModel.Cancel(string.Format("Pay {0}", ManaCost));
                Model.Interaction.UserChoiceSelected += Interaction_UserChoiceSelected;

                TagManaThatCanBePaid();
                PlayerViewModel.ManaPaid += Interaction_ManaPaid;
            }

            protected override void End()
            {
                Model.Interaction.UserChoiceSelected -= Interaction_UserChoiceSelected;
                PlayerViewModel.ManaPaid -= Interaction_ManaPaid;

                base.End();
            }

            private void TagManaThatCanBePaid()
            {
                Debug.Assert(ManaCost.IsConcrete);

                if (ManaCost.Colorless > 0)
                {
                    foreach (Color color in Enum.GetValues(typeof(Color)))
                    {
                        if (ManaPool.Mana[color] > 0)
                        {
                            ManaPool.CanPay[color] = true;
                        }
                    }
                }

                foreach (ManaSymbol symbol in ManaCost.Symbols)
                {
                    Color color = ManaSymbolHelper.GetColor(symbol);
                    if (ManaPool.Mana[color] > 0)
                    {
                        ManaPool.CanPay[color] = true;
                    }
                }
            }

            #endregion

            #region Event Handlers

            void Interaction_UserChoiceSelected(object sender, ItemEventArgs<UserChoiceModel> e)
            {
                Debug.Assert(e.Item.Type == UserChoiceType.Cancel);
                Result = null;
                End();
            }

            void Interaction_ManaPaid(object sender, ItemEventArgs<Color> e)
            {
                ManaPayment payment = new ManaPayment();
                payment.Pay(e.Item);

                Result = new PayManaAction(payment);
                End();
            }

            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        /// Begins a "pay mana" interaction.
        /// </summary>
        /// <returns></returns>
        public IInteraction<Action> BeginPayMana(ManaCost manaCost)
        {
            return BeginInteraction<PayManaInteraction>(interaction => interaction.ManaCost = manaCost);
        }

        #endregion
    }
}
