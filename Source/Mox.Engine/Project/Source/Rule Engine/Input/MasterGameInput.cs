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
using System.Collections.Generic;

namespace Mox.Flow
{
    /// <summary>
    /// Master game controller.
    /// </summary>
    /// <remarks>
    /// Dispatches to client controllers or to AI.
    /// </remarks>
    public class MasterGameInput : IChoiceDecisionMaker
    {
        #region Variables

        private readonly Game m_game;
        private IChoiceDecisionMaker m_fallback = new DeadGameInput();

        private readonly Dictionary<Player, IChoiceDecisionMaker> m_inputs = new Dictionary<Player, IChoiceDecisionMaker>();

        #endregion

        #region Constructor

        public MasterGameInput(Game game)
        {
            Throw.IfNull(game, "game");
            m_game = game;
        }

        #endregion

        #region Properties

        public IChoiceDecisionMaker Fallback
        {
            get { return m_fallback; }
            set
            {
                Throw.IfNull(value, "Fallback");
                m_fallback = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Assigns the given <paramref name="input"/> to the given <paramref name="player"/>.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="input"></param>
        public void AssignClientInput(Player player, IClientInput input)
        {
            Throw.IfNull(player, "player");
            Throw.IfNull(input, "input");
            ValidatePlayer(player);

            m_inputs[player] = new ClientInputRouter(m_game, input);
        }

        /// <summary>
        /// Assigns the given <paramref name="input"/> to the given <paramref name="player"/>.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="input"></param>
        public void AssignClientInput(Player player, IChoiceDecisionMaker input)
        {
            Throw.IfNull(player, "player");
            Throw.IfNull(input, "input");
            ValidatePlayer(player);

            m_inputs[player] = input;
        }

        /// <summary>
        /// Unassigns the specific controller associated with the given <paramref name="player"/>, if any.
        /// </summary>
        /// <param name="player"></param>
        public void Unassign(Player player)
        {
            Throw.IfNull(player, "player");
            ValidatePlayer(player);

            m_inputs.Remove(player);
        }

        private IChoiceDecisionMaker GetInput(Player player)
        {
            ValidatePlayer(player);

            IChoiceDecisionMaker input;
            if (!m_inputs.TryGetValue(player, out input))
            {
                input = m_fallback;
            }

            return input;
        }

        private void ValidatePlayer(Player player)
        {
            Throw.InvalidArgumentIf(player.Manager != m_game, "Player is from another game!", "player");
        }

        #endregion

        #region Implementation of IChoiceDecisionMaker

        public object MakeChoiceDecision(Sequencer sequencer, Choice choice)
        {
            return GetInput(choice.Player.Resolve(m_game)).MakeChoiceDecision(sequencer, choice);
        }

        #endregion
    }
}
