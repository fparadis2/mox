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

        private class AskModalChoiceInteraction : Interaction<ModalChoiceResult>
        {
            public ModalChoiceContext Context
            {
                get;
                set;
            }

            public override void Run()
            {
                base.Run();

                // TODO: Use Context.Importance to not constantly harass user with trivial choices (user option)
                // TODO: Use the default choice

                UserChoiceInteractionModel model = new UserChoiceInteractionModel 
                { 
                    Text = Context.Question 
                };

                foreach (var choice in Context.Choices)
                {
                    model.Choices.Add(new UserChoiceModel
                    {
                        Text = choice.ToString()
                    });
                }

                Model.Interaction.UserChoiceSelected += Interaction_UserChoiceSelected;
                Model.Interaction.UserChoiceInteraction = model;
            }

            protected override void End()
            {
                Model.Interaction.UserChoiceSelected -= Interaction_UserChoiceSelected;

                base.End();
            }

            private void Interaction_UserChoiceSelected(object sender, ItemEventArgs<UserChoiceModel> e)
            {
                Result = (ModalChoiceResult)Enum.Parse(typeof(ModalChoiceResult), e.Item.Text);
                End();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Begins a modal choice interaction.
        /// </summary>
        /// <returns></returns>
        public IInteraction<ModalChoiceResult> BeginAskModalChoice(ModalChoiceContext context)
        {
            return BeginInteraction<AskModalChoiceInteraction>(interaction => interaction.Context = context);
        }

        #endregion
    }
}
