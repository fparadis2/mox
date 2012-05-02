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

namespace Mox.UI.Game
{
    partial class InteractionController
    {
        private class TargetInteraction : Interaction
        {
            #region Properties

            /// <summary>
            /// The possible targets.
            /// </summary>
            public IList<int> Targets { get; set; }

            /// <summary>
            /// Whether we allow the user to cancel the targetting.
            /// </summary>
            public bool AllowCancel { get; set; }

            /// <summary>
            /// Type of targeting.
            /// </summary>
            public TargetContextType TargetContextType { get; set; }

            #endregion

            #region Methods

            public override void Run()
            {
                base.Run();

                List<UserChoiceModel> choices = new List<UserChoiceModel>();
                if (AllowCancel)
                {
                    choices.Add(new UserChoiceModel { Text = "Cancel", Type = UserChoiceType.Cancel });
                }

                UserChoiceInteractionModel interactionModel = new UserChoiceInteractionModel(choices.ToArray())
                {
                    Text = GetText(TargetContextType)
                };

                Model.Interaction.UserChoiceInteraction = interactionModel;

                TagObjectsThatCanBeTargeted();

                Model.Interaction.UserChoiceSelected += Interaction_UserChoiceSelected;
                Model.Interaction.CardChosen += Interaction_CardChosen;
                Model.Interaction.PlayerChosen += Interaction_PlayerChosen;
            }

            protected override void End(object result)
            {
                Model.Interaction.UserChoiceSelected -= Interaction_UserChoiceSelected;
                Model.Interaction.CardChosen -= Interaction_CardChosen;
                Model.Interaction.PlayerChosen -= Interaction_PlayerChosen;

                base.End(result);
            }

            private bool CanBeTargeted(int identifier)
            {
                return Targets.Contains(identifier);
            }

            private void TagObjectsThatCanBeTargeted()
            {
                foreach (PlayerViewModel player in Model.Players)
                {
                    player.CanBeChosen = CanBeTargeted(player.Identifier);
                }

                foreach (CardViewModel card in Model.AllCards)
                {
                    card.CanChoose = CanBeTargeted(card.Identifier);
                }
            }

            private static string GetText(TargetContextType type)
            {
                switch (type)
                {
                    case TargetContextType.Normal:
                        return "Target... (TODO: a better message!)";

                    case TargetContextType.Discard:
                        return "Discard a card";

                    default:
                        throw new NotImplementedException();
                }
            }

            #endregion

            #region Event Handlers

            void Interaction_UserChoiceSelected(object sender, ItemEventArgs<UserChoiceModel> e)
            {
                Debug.Assert(e.Item.Type == UserChoiceType.Cancel);
                End(ObjectManager.InvalidIdentifier);
            }

            private void Interaction_CardChosen(object sender, CardChosenEventArgs e)
            {
                End(e.Card.Identifier);
            }

            private void Interaction_PlayerChosen(object sender, PlayerChosenEventArgs e)
            {
                End(e.Player.Identifier);
            }

            #endregion
        }
    }
}
