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
using System.Linq;

namespace Mox.UI.Game
{
    partial class InteractionController
    {
        /// <summary>
        /// Base class for interactions that allow user to play card abilities.
        /// </summary>
        private abstract class PlayCardInteraction : Interaction
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

                Controller.TagCardsThatCanBePlayed(CreateEvaluationContext());
                Model.Interaction.CardChosen += Interaction_CardChosen;
            }

            protected override void End(object result)
            {
                Model.Interaction.CardChosen -= Interaction_CardChosen;
                base.End(result);
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
                End(new PlayAbility(e.Card.Source.Abilities.First(ability => ability.CanPlay(Player, CreateEvaluationContext()))));
            }

            #endregion
        }

        private class GivePriorityInteraction : PlayCardInteraction
        {
            #region Methods

            public override bool Skip(out object result)
            {
                var step = Model.State.Step;
                if (!ShouldStop(step) || !Model.SpellStack.IsEmpty)
                {
                    result = null;
                    return true;
                }

                return base.Skip(out result);
            }

            public override void Run()
            {
                base.Run();

                Model.Interaction.UserChoiceInteraction = UserChoiceInteractionModel.Cancel("Play a card or pass...", "Pass");
                Model.Interaction.UserChoiceSelected += Interaction_UserChoiceSelected;
            }

            protected override void End(object result)
            {
                Model.Interaction.UserChoiceSelected -= Interaction_UserChoiceSelected;

                base.End(result);
            }

            private bool ShouldStop(StepViewModel step)
            {
                if (Model.IsActivePlayer)
                    return step.StopOnMyTurn;

                return step.StopOnOpponentTurn;
            }

            #endregion

            #region Event Handlers

            void Interaction_UserChoiceSelected(object sender, ItemEventArgs<UserChoiceModel> e)
            {
                Debug.Assert(e.Item.Type == UserChoiceType.Cancel);
                End(null);
            }

            #endregion
        }
    }
}
