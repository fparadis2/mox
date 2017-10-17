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
using System.Linq;

namespace Mox.UI.Game
{
    partial class InteractionController
    {
        private class DeclareAttackersInteraction : Interaction
        {
            #region Variables

            private readonly List<CardViewModel> m_selectedAttackers = new List<CardViewModel>();

            #endregion

            #region Properties

            public DeclareAttackersContext AttackInfo
            {
                get;
                set;
            }

            #endregion

            #region Methods

            public override void Run()
            {
                base.Run();

                Model.Interaction.UserChoiceInteraction = new UserChoiceInteractionModel { Text = "Declare your attackers" };
                Model.Interaction.UserChoiceSelected += Interaction_UserChoiceSelected;

                Model.Interaction.CardChosen += Interaction_CardChosen;

                TagPossibleAttackers();
                RefreshChoices();
            }

            protected override void End(object result)
            {
                Model.Interaction.CardChosen -= Interaction_CardChosen;

                Model.Interaction.UserChoiceSelected -= Interaction_UserChoiceSelected;

                foreach (CardViewModel cardViewModel in Model.AllCards)
                {
                    cardViewModel.IsAttacking = false;
                }

                base.End(result);
            }

            private void RefreshChoices()
            {
                Model.Interaction.UserChoiceInteraction.Choices.Clear();

                if (m_selectedAttackers.Count > 0)
                {
                    Model.Interaction.UserChoiceInteraction.Choices.Add(new UserChoiceModel { Text = "Attack", Type = UserChoiceType.Yes });
                    Model.Interaction.UserChoiceInteraction.Choices.Add(new UserChoiceModel { Text = "Cancel", Type = UserChoiceType.No });
                }
                else
                {
                    Model.Interaction.UserChoiceInteraction.Choices.Add(new UserChoiceModel { Text = "Pass", Type = UserChoiceType.Cancel });
                }
            }

            private void TagPossibleAttackers()
            {
                foreach (CardViewModel cardViewModel in Model.AllCards)
                {
                    cardViewModel.IsAttacking = false;

                    if (AttackInfo.LegalAttackers.Contains(cardViewModel.Identifier))
                    {
                        cardViewModel.InteractionType = InteractionType.Attack;
                    }
                    else
                    {
                        cardViewModel.InteractionType = InteractionType.None;
                    }
                }
            }

            #endregion

            #region Event Handlers

            void Interaction_UserChoiceSelected(object sender, ItemEventArgs<UserChoiceModel> e)
            {
                switch (e.Item.Type)
                {
                    case UserChoiceType.No:
                        m_selectedAttackers.Clear();
                        RefreshChoices();
                        break;

                    case UserChoiceType.Yes:
                        End(new DeclareAttackersResult(m_selectedAttackers.Select(cvm => cvm.Source).ToArray()));
                        break;

                    case UserChoiceType.Cancel:
                        End(new DeclareAttackersResult());
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            void Interaction_CardChosen(object sender, CardChosenEventArgs e)
            {
                if (m_selectedAttackers.Remove(e.Card))
                {
                    e.Card.IsAttacking = false;
                }
                else
                {
                    m_selectedAttackers.Add(e.Card);
                    e.Card.IsAttacking = true;
                }

                RefreshChoices();
            }

            #endregion
        }
    }
}
