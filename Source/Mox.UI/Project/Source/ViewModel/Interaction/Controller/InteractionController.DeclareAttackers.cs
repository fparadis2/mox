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

        private class DeclareAttackersInteraction : Interaction<DeclareAttackersResult>
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

                Refresh();
            }

            protected override void End()
            {
                Model.Interaction.CardChosen -= Interaction_CardChosen;

                Model.Interaction.UserChoiceSelected -= Interaction_UserChoiceSelected;

                base.End();
            }

            private void Refresh()
            {
                SetupChoices();
                TagPossibleAttackers();
            }

            private void SetupChoices()
            {
                Model.Interaction.UserChoiceInteraction.Choices.Clear();

                if (m_selectedAttackers.Count > 0)
                {
                    Model.Interaction.UserChoiceInteraction.Choices.Add(new UserChoiceModel { Text = "Continue", Type = UserChoiceType.Yes });
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
                    cardViewModel.CanBeChosen = AttackInfo.LegalAttackers.Contains(cardViewModel.Identifier) && !m_selectedAttackers.Contains(cardViewModel);
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
                        Refresh();
                        break;

                    case UserChoiceType.Yes:
                        Result = new DeclareAttackersResult(m_selectedAttackers.Select(cvm => cvm.Source).ToArray());
                        End();
                        break;

                    case UserChoiceType.Cancel:
                        Result = new DeclareAttackersResult();
                        End();
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            void Interaction_CardChosen(object sender, CardChosenEventArgs e)
            {
                m_selectedAttackers.Add(e.Card);
                SetupChoices();
                TagPossibleAttackers();
            }

            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        /// Begins a "declare atackers" interaction.
        /// </summary>
        /// <returns></returns>
        public IInteraction<DeclareAttackersResult> BeginDeclareAttackers(DeclareAttackersContext attackInfo)
        {
            return BeginInteraction<DeclareAttackersInteraction>(interaction => interaction.AttackInfo = attackInfo);
        }

        #endregion
    }
}
