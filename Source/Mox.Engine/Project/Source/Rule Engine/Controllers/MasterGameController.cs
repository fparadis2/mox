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
using System.Reflection;
using System.Text;

namespace Mox.Flow
{
    /// <summary>
    /// Master game controller.
    /// </summary>
    /// <remarks>
    /// Dispatches to client controllers or to AI.
    /// </remarks>
    public class MasterGameController : IGameController
    {
        #region Inner Types

        /// <summary>
        /// Makes the bridge between an <see cref="IGameController"/> and a <see cref="IClientController"/>.
        /// </summary>
        private class ClientGameController : IGameController
        {
            #region Variables

            private readonly IClientController m_controller;

            #endregion

            #region Constructor

            public ClientGameController(IClientController controller)
            {
                Throw.IfNull(controller, "controller");
                m_controller = controller;
            }

            #endregion

            #region Implementation of IGameController

            public ModalChoiceResult AskModalChoice(Part<IGameController>.Context context, Player player, ModalChoiceContext choiceContext)
            {
                return m_controller.AskModalChoice(choiceContext);
            }

            /// <summary>
            /// Gives the priority to the given <paramref name="player"/>.
            /// </summary>
            /// <returns>The action to do, null otherwise (to pass).</returns>
            public Action GivePriority(Part<IGameController>.Context context, Player player)
            {
                return m_controller.GivePriority();
            }

            /// <summary>
            /// Asks the given <paramref name="player"/> to pay for mana.
            /// </summary>
            /// <param name="context"></param>
            /// <param name="player"></param>
            /// <param name="manaCost">The cost to pay.</param>
            /// <returns>The action to do (either a mana ability or a mana payment), null otherwise (to cancel).</returns>
            public Action PayMana(Part<IGameController>.Context context, Player player, ManaCost manaCost)
            {
                return m_controller.PayMana(manaCost);
            }

            /// <summary>
            /// Asks the player to choose a target from the possible targets.
            /// </summary>
            /// <param name="context"></param>
            /// <param name="player"></param>
            /// <param name="targetInfo"></param>
            /// <returns></returns>
            public int Target(Part<IGameController>.Context context, Player player, TargetContext targetInfo)
            {
                return m_controller.Target(targetInfo);
            }

            /// <summary>
            /// Asks the player whether to mulligan.
            /// </summary>
            /// <returns>True to mulligan.</returns>
            public bool Mulligan(Part<IGameController>.Context context, Player player)
            {
                return m_controller.Mulligan();
            }

            public DeclareAttackersResult DeclareAttackers(Part<IGameController>.Context context, Player player, DeclareAttackersContext attackInfo)
            {
                return m_controller.DeclareAttackers(attackInfo);
            }

            public DeclareBlockersResult DeclareBlockers(Part<IGameController>.Context context, Player player, DeclareBlockersContext blockInfo)
            {
                return m_controller.DeclareBlockers(blockInfo);
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly Game m_game;
        private readonly IGameController m_fallbackController;

        private readonly Dictionary<Player, IGameController> m_controllers = new Dictionary<Player, IGameController>();

        #endregion

        #region Constructor

        public MasterGameController(Game game)
            : this(game, new DeadGameController())
        {
        }

        public MasterGameController(Game game, IGameController fallbackController)
        {
            Throw.IfNull(game, "game");
            Throw.IfNull(fallbackController, "fallbackController");

            m_game = game;
            m_fallbackController = fallbackController;
        }

        #endregion

        #region Properties

        public IGameController FallbackController
        {
            get { return m_fallbackController; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Assigns the given <paramref name="controller"/> to the given <paramref name="player"/>.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="controller"></param>
        public void AssignClientController(Player player, IClientController controller)
        {
            Throw.IfNull(player, "player");
            Throw.IfNull(controller, "controller");
            ValidatePlayer(player);

            m_controllers[player] = new ClientGameController(controller);
        }

        /// <summary>
        /// Unassigns the specific controller associated with the given <paramref name="player"/>, if any.
        /// </summary>
        /// <param name="player"></param>
        public void Unassign(Player player)
        {
            Throw.IfNull(player, "player");
            ValidatePlayer(player);

            m_controllers.Remove(player);
        }

        private IGameController GetController(Player player)
        {
            ValidatePlayer(player);

            IGameController controller;
            if (!m_controllers.TryGetValue(player, out controller))
            {
                controller = m_fallbackController;
            }

            return controller;
        }

        private void ValidatePlayer(Player player)
        {
            Throw.InvalidArgumentIf(player.Manager != m_game, "Player is from another game!", "player");
        }

        #endregion

        #region Implementation of IGameController

        public ModalChoiceResult AskModalChoice(Part<IGameController>.Context context, Player player, ModalChoiceContext choiceContext)
        {
            return GetController(player).AskModalChoice(context, player, choiceContext);
        }

        /// <summary>
        /// Gives the priority to the given <paramref name="player"/>.
        /// </summary>
        /// <returns>The action to do, null otherwise (to pass).</returns>
        Action IGameController.GivePriority(Part<IGameController>.Context context, Player player)
        {
            return GetController(player).GivePriority(context, player);
        }

        /// <summary>
        /// Asks the given <paramref name="player"/> to pay for mana.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="player"></param>
        /// <param name="manaCost">The cost to pay.</param>
        /// <returns>The action to do (either a mana ability or a mana payment), null otherwise (to cancel).</returns>
        Action IGameController.PayMana(Part<IGameController>.Context context, Player player, ManaCost manaCost)
        {
            return GetController(player).PayMana(context, player, manaCost);
        }

        /// <summary>
        /// Asks the player to choose a target from the possible targets.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="player"></param>
        /// <param name="targetInfo"></param>
        /// <returns></returns>
        int IGameController.Target(Part<IGameController>.Context context, Player player, TargetContext targetInfo)
        {
            return GetController(player).Target(context, player, targetInfo);
        }

        /// <summary>
        /// Asks the player whether to mulligan.
        /// </summary>
        /// <returns>True to mulligan.</returns>
        bool IGameController.Mulligan(Part<IGameController>.Context context, Player player)
        {
            return GetController(player).Mulligan(context, player);
        }

        public DeclareAttackersResult DeclareAttackers(Part<IGameController>.Context context, Player player, DeclareAttackersContext attackInfo)
        {
            return GetController(player).DeclareAttackers(context, player, attackInfo);
        }

        public DeclareBlockersResult DeclareBlockers(Part<IGameController>.Context context, Player player, DeclareBlockersContext blockInfo)
        {
            return GetController(player).DeclareBlockers(context, player, blockInfo);
        }

        #endregion
    }
}
