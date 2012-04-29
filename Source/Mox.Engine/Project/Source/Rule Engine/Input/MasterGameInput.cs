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
        #region Inner Types

        /// <summary>
        /// Makes the bridge between an <see cref="IChoiceDecisionMaker"/> and a <see cref="IClientInput"/>.
        /// </summary>
        private class ClientInput : IChoiceDecisionMaker
        {
            #region Variables

            private readonly Game m_game;
            private readonly IClientInput m_clientInput;
            private static readonly Dictionary<System.Type, Dispatcher> ms_dispatchers = new Dictionary<System.Type, Dispatcher>();

            #endregion

            #region Constructor

            static ClientInput()
            {
                ms_dispatchers.Add(typeof (ModalChoice), (i, g, c) => i.AskModalChoice(((ModalChoice)c).Context));
                ms_dispatchers.Add(typeof (GivePriorityChoice), (i, g, c) => i.GivePriority());
                ms_dispatchers.Add(typeof (PayManaChoice), (i, g, c) => i.PayMana(((PayManaChoice)c).ManaCost));
                ms_dispatchers.Add(typeof (MulliganChoice), (i, g, c) => i.Mulligan());
                ms_dispatchers.Add(typeof (TargetChoice), (i, g, c) => i.Target(((TargetChoice)c).Context));
                ms_dispatchers.Add(typeof (DeclareAttackersChoice), (i, g, c) => i.DeclareAttackers(((DeclareAttackersChoice)c).AttackContext));
                ms_dispatchers.Add(typeof (DeclareBlockersChoice), (i, g, c) => i.DeclareBlockers(((DeclareBlockersChoice)c).BlockContext));
            }

            public ClientInput(Game game, IClientInput clientInput)
            {
                Throw.IfNull(game, "game");
                Throw.IfNull(clientInput, "clientInput");

                m_game = game;
                m_clientInput = clientInput;
            }

            #endregion

            #region Implementation of IChoiceDecisionMaker

            public object MakeChoiceDecision(Sequencer sequencer, Choice choice)
            {
                Dispatcher dispatcher;
                if (!ms_dispatchers.TryGetValue(choice.GetType(), out dispatcher))
                {
                    throw new NotImplementedException(string.Format("Unknown choice: {0}", choice));
                }

                return dispatcher(m_clientInput, m_game, choice);
            }

            #endregion

            #region Inner Types

            private delegate object Dispatcher(IClientInput controller, Game game, Choice choice);

            #endregion
        }

        #endregion

        #region Variables

        private readonly Game m_game;
        private readonly IChoiceDecisionMaker m_fallback;

        private readonly Dictionary<Player, IChoiceDecisionMaker> m_inputs = new Dictionary<Player, IChoiceDecisionMaker>();

        #endregion

        #region Constructor

        public MasterGameInput(Game game)
            : this(game, new DeadGameInput())
        {
        }

        public MasterGameInput(Game game, IChoiceDecisionMaker fallback)
        {
            Throw.IfNull(game, "game");
            Throw.IfNull(fallback, "fallback");

            m_game = game;
            m_fallback = fallback;
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

            m_inputs[player] = new ClientInput(m_game, input);
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
