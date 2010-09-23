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
using System.Windows.Threading;

namespace Mox.UI
{
    public class UIPlayerController : IClientController
    {
        #region Variables
        
        private readonly InteractionController m_interactionController;

        #endregion

        #region Constructor

        public UIPlayerController(GameViewModel gameViewModel, Dispatcher dispatcher)
            : this(new InteractionController(gameViewModel, dispatcher))
        {
        }

        internal UIPlayerController(InteractionController interactionController)
        {
            Throw.IfNull(interactionController, "interactionController");
            m_interactionController = interactionController;
        }

        #endregion

        #region Implementation of IClientController

        public ModalChoiceResult AskModalChoice(ModalChoiceContext context)
        {
            IInteraction<ModalChoiceResult> interaction = m_interactionController.BeginAskModalChoice(context);
            interaction.Wait();
            return interaction.Result;
        }

        /// <summary>
        /// Gives the priority to this controller.
        /// </summary>
        /// <returns>The action to do, null otherwise (to pass).</returns>
        public Action GivePriority()
        {
            IInteraction<Action> interaction = m_interactionController.BeginGivePriority();
            interaction.Wait();
            return interaction.Result;
        }

        /// <summary>
        /// Asks the player to pay for mana.
        /// </summary>
        /// <returns>The action to do (either a mana ability or a mana payment), null otherwise (to cancel).</returns>
        public Action PayMana(ManaCost cost)
        {
            IInteraction<Action> interaction = m_interactionController.BeginPayMana(cost);
            interaction.Wait();
            return interaction.Result;
        }

        /// <summary>
        /// Asks the player whether to mulligan.
        /// </summary>
        /// <returns>True to mulligan.</returns>
        public bool Mulligan()
        {
            IInteraction<bool> interaction = m_interactionController.BeginMulligan();
            interaction.Wait();
            return interaction.Result;
        }

        /// <summary>
        /// Asks the player to choose a target from the possible targets.
        /// </summary>
        /// <returns></returns>
        public int Target(TargetContext targetContext)
        {
            IInteraction<int> interaction = m_interactionController.BeginTarget(targetContext);
            interaction.Wait();
            return interaction.Result;
        }

        public DeclareAttackersResult DeclareAttackers(DeclareAttackersContext attackInfo)
        {
            IInteraction<DeclareAttackersResult> interaction = m_interactionController.BeginDeclareAttackers(attackInfo);
            interaction.Wait();
            return interaction.Result;
        }

        public DeclareBlockersResult DeclareBlockers(DeclareBlockersContext blockInfo)
        {
            // TODO
            return DeclareBlockersResult.Empty;
        }

        #endregion
    }
}