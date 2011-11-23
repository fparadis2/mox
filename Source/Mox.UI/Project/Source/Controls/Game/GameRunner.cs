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
using Mox.Replication;

namespace Mox.UI
{
    public class GameRunner
    {
        #region Variables

        private readonly GameEngine m_gameEngine;
        private readonly ReplicationSource<Player> m_replicationSource;

        #endregion

        #region Constructor

        public GameRunner()
        {
            // Create game
            Game game = new Game();

            // Add Players
            Player playerA = game.CreatePlayer(); playerA.Name = "Player A";
            Player playerB = game.CreatePlayer(); playerB.Name = "Player B";

            GameInitializer initializer = new GameInitializer(MasterCardFactory.Instance);
            initializer.AssignDeck(playerA, Decks.Create_Kamahls_Temper());
            initializer.AssignDeck(playerB, Decks.Create_ChoMannosResolve());
            initializer.Initialize(game);

            // Give a slight advantage to player A for debugging purposes
            foreach (Color color in Enum.GetValues(typeof(Color)))
            {
                playerA.ManaPool[color] = 10;
            }

            m_replicationSource = new ReplicationSource<Player>(game, new OpenAccessControlStrategy<Player>());
            m_gameEngine = new GameEngine(game);
            m_gameEngine.AISupervisor.Parameters.GlobalAITimeout = TimeSpan.FromSeconds(10);
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        public Game Replicate()
        {
            return m_replicationSource.Register<Game>(m_gameEngine.Game.Players[0]);
        }

        public void AssignController(Resolvable<Player> player, IClientInput controller)
        {
            Player realPlayer = player.Resolve(m_gameEngine.Game);
            m_gameEngine.Input.AssignClientInput(realPlayer, controller);
        }

        public void Run()
        {
            m_gameEngine.Run(m_gameEngine.Game.Players[0]);
        }

        #endregion
    }
}