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
using Mox.Database;

namespace Mox.AI.Arena
{
    public class Arena
    {
        #region Variables

        private readonly int m_seed;

        private readonly Game m_game;
        private readonly Player m_playerA;
        private readonly Player m_playerB;

        private readonly GameEngine m_gameEngine;
        private readonly AISupervisor m_aiSupervisor;

        #endregion

        #region Constructor

        public Arena(int seed)
        {
            m_seed = seed;
            m_game = new Game();

            m_playerA = m_game.CreatePlayer();
            m_playerA.Name = "Player A";

            m_playerB = m_game.CreatePlayer();
            m_playerB.Name = "Player B";

            m_gameEngine = new GameEngine(m_game);

            m_aiSupervisor = new AISupervisor(m_game);
            m_aiSupervisor.Parameters.GlobalAITimeout = TimeSpan.FromSeconds(1);

            m_gameEngine.Input.Fallback = m_aiSupervisor;
        }

        #endregion

        #region Properties

        public Game Game
        {
            get { return m_game; }
        }

        public Deck DeckA
        {
            get;
            set;
        }

        public Deck DeckB
        {
            get;
            set;
        }

        #endregion

        #region Methods

        public ArenaResult Run()
        {
            Initialize();

            m_gameEngine.Run(m_playerA);

            return Analyze();
        }

        private void Initialize()
        {
            GameInitializer initializer = new GameInitializer(MasterCardFactory.Instance, MasterCardDatabase.Instance)
            {
                Seed = m_seed
            };

            initializer.AssignDeck(m_playerA, DeckA);
            initializer.AssignDeck(m_playerB, DeckB);
            initializer.Initialize(m_game);
        }

        private ArenaResult Analyze()
        {
            return new ArenaResult
            {
                Seed = m_seed,
                Winner = m_game.State.Winner,
                WinnerScore = ComputeHeuristic(m_game.State.Winner)
            };
        }

        private float ComputeHeuristic(Player maximizingPlayer)
        {
            return m_aiSupervisor.CreateAlgorithm(maximizingPlayer).ComputeHeuristic(m_game, false);
        }

        #endregion
    }
}
