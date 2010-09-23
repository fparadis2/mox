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

        /// <summary>
        /// Base class for interactions that allow user to play card abilities.
        /// </summary>
        private abstract class PlayCardInteraction : Interaction<Action>
        {
            #region Properties

            /// <summary>
            /// The context used to evaluate which abilities can be played.
            /// </summary>
            protected virtual EvaluationContextType EvaluationContextType
            {
                get 
                {
                    return EvaluationContextType.Normal;
                }
            }

            #endregion

            #region Methods

            public override void Run()
            {
                base.Run();

                Controller.TagCardThatCanBePlayed(CreateEvaluationContext());
                Model.Interaction.CardChosen += Interaction_CardChosen;
            }

            protected override void End()
            {
                Model.Interaction.CardChosen -= Interaction_CardChosen;
                base.End();
            }

            private ExecutionEvaluationContext CreateEvaluationContext()
            {
                return new ExecutionEvaluationContext
                {
                    Type = EvaluationContextType,
                    UserMode = true
                };
            }

            #endregion

            #region Event Handlers

            private void Interaction_CardChosen(object sender, CardChosenEventArgs e)
            {
                Result = new PlayAbility(e.Card.Source.Abilities.First(ability => ability.CanPlay(Player, CreateEvaluationContext())));
                End();
            }

            #endregion
        }

        private class GivePriorityInteraction : PlayCardInteraction
        {
            #region Methods

            public override void Run()
            {
                base.Run();

                Model.Interaction.UserChoiceInteraction = UserChoiceInteractionModel.Cancel("Play a card or pass...", "Pass");
                Model.Interaction.UserChoiceSelected += Interaction_UserChoiceSelected;
            }

            protected override void End()
            {
                Model.Interaction.UserChoiceSelected -= Interaction_UserChoiceSelected;

                base.End();
            }

            #endregion

            #region Event Handlers

            void Interaction_UserChoiceSelected(object sender, ItemEventArgs<UserChoiceModel> e)
            {
                Debug.Assert(e.Item.Type == UserChoiceType.Cancel);
                Result = null;
                End();
            }

            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        /// Begins a "give priority" interaction.
        /// </summary>
        /// <returns></returns>
        public IInteraction<Action> BeginGivePriority()
        {
            return BeginInteraction<GivePriorityInteraction>();
        }

        #endregion
    }
}
