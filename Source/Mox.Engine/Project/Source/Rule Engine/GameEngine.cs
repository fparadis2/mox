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
using Mox.AI;
using Mox.Flow;
using Mox.Flow.Parts;

namespace Mox
{
    /// <summary>
    /// Main entry point for playing a game.
    /// </summary>
    public class GameEngine
    {
        #region Variables

        private readonly MasterGameInput m_input;
        private readonly Game m_game;

        #endregion
         
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameEngine(Game game)
        {
            Throw.IfNull(game, "game");

            m_game = game;

            m_input = new MasterGameInput(game);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Controller
        /// </summary>
        public MasterGameInput Input
        {
            get { return m_input; }
        }

        /// <summary>
        /// Game.
        /// </summary>
        public Game Game
        {
            get { return m_game; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Runs the game.
        /// </summary>
        public void Run(Player startingPlayer, ICancellable cancellable = null)
        {
            var sequencer = new Sequencer(m_game, new GameFlow(startingPlayer));
            sequencer.Run(m_input, cancellable);
        }

        #endregion
    }
}