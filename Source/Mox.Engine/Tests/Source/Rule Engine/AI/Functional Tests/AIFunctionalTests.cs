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
using Mox.Flow.Phases;
using Mox.Rules;
using NUnit.Framework;

using Mox.Flow;
using Mox.Flow.Parts;

namespace Mox.AI.Functional
{
    public abstract class AIFunctionalTests
    {
        #region Variables

        private readonly ICardFactory m_factory = new AssemblyCardFactory(typeof(ICardFactory).Assembly);

        protected Game m_game;
        protected Player m_playerA;
        protected Player m_playerB;

        private IGameController m_controller;
        private AISupervisor<IGameController> m_supervisor;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public virtual void Setup()
        {
            m_game = new Game();

            // Add Players
            m_playerA = m_game.CreatePlayer(); m_playerA.Name = "Player A";
            m_playerB = m_game.CreatePlayer(); m_playerB.Name = "Player B";

            m_supervisor = new AISupervisor<IGameController>(m_game);
            m_controller = new MasterGameController(m_game, m_supervisor.AIController);
        }

        [TearDown]
        public virtual void Teardown()
        {
            m_supervisor.Dispose();
        }

        protected void SetupGame()
        {
            new GameInitializer(m_factory).Initialize(m_game);
            m_game.State.CurrentPhase = Phases.PrecombatMain;
        }

        #endregion

        #region Utilities

        #region Flow

        protected void Run(Part<IGameController> part)
        {
            ToSequencer(part).Run(m_controller);
        }

        protected void RunUntil<TStep>(Part<IGameController> part)
            where TStep : Step
        {
            RunUntil(ToSequencer(part), m_controller, IsNotStep<TStep>);
        }

        /// <summary>
        /// Runs until a part fails the given <paramref name="test"/>.
        /// </summary>
        /// <returns></returns>
        private static bool RunUntil<TController>(Sequencer<TController> sequencer, TController controller, Func<Part<TController>, bool> test)
        {
            Throw.IfNull(test, "test");

            while (!sequencer.IsEmpty)
            {
                if (!test(sequencer.NextPart) || sequencer.RunOnce(controller) == SequencerResult.Stop)
                {
                    break;
                }
            }

            return sequencer.IsEmpty;
        }

        private static bool IsNotStep<TStep>(Part<IGameController> part)
            where TStep : Step
        {
            SequenceStep sequence = part as SequenceStep;
            if (sequence != null)
            {
                return !(sequence.Step is TStep);
            }
            return true;
        }

        private Sequencer<IGameController> ToSequencer(Part<IGameController> part)
        {
            return new Sequencer<IGameController>(part, m_game);
        }

        protected void Play_until_all_players_pass_and_the_stack_is_empty(Player startingPlayer, bool oneLandPerTurn)
        {
            // "Emulate" a turn loop

            foreach (Player player in Player.Enumerate(startingPlayer, false))
            {
                m_game.State.ActivePlayer = player;
                m_game.TurnData.ResetAllValues();

                using (oneLandPerTurn ? null : OneLandPerTurn.Bypass())
                {
                    Run(new PlayUntilAllPlayersPassAndTheStackIsEmpty(player));
                }
            }
        }

        protected void Do_Combat(Player player)
        {
            Phase phase = DefaultTurnFactory.CreateCombatPhase();
            Run(new SequencePhase(player, phase));
        }

        #endregion

        #region Misc

        protected Card AddCard(Player player, Zone zone, string set, string cardName)
        {
            Card card = m_game.CreateCard(player, new CardIdentifier { Card = cardName });
            card.Zone = zone;
            return card;
        }

        protected static IDisposable Profile()
        {
            Stopwatch timer = Stopwatch.StartNew();

            return new DisposableHelper(() =>
            {
                timer.Stop();
                Console.WriteLine("Total time: " + timer.Elapsed.TotalSeconds + "s");
            });
        }

        #endregion

        #endregion
    }
}
