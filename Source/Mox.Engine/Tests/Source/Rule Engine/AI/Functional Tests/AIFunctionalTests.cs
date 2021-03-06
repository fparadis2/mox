﻿// Copyright (c) François Paradis
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
using System.Diagnostics;
using Mox.Database;
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

        private readonly ICardFactory m_factory = MasterCardFactory.Instance;

        protected Game m_game;
        protected Player m_playerA;
        protected Player m_playerB;

        private IChoiceDecisionMaker m_decisionMaker;
        private AISupervisor m_supervisor;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public virtual void Setup()
        {
            m_game = new Game();

            // Add Players
            m_playerA = m_game.CreatePlayer(); m_playerA.Name = "Player A";
            m_playerB = m_game.CreatePlayer(); m_playerB.Name = "Player B";

            m_supervisor = new AISupervisor(m_game);
            m_decisionMaker = new MasterGameInput(m_game) { Fallback = m_supervisor };
        }

        [TearDown]
        public virtual void Teardown()
        {
            m_supervisor.Dispose();
        }

        protected void SetupGame()
        {
            new GameInitializer(m_factory, MasterCardDatabase.Instance).Initialize(m_game);
            m_game.State.CurrentPhase = Phases.PrecombatMain;
        }

        protected void InitializeCard(Card card)
        {
            var result = MasterCardFactory.Initialize(card, m_factory, MasterCardDatabase.Instance);
            Assert.AreEqual(CardFactoryResult.ResultType.Success, result.Type);
        }

        #endregion

        #region Utilities

        #region Flow

        protected void Run(Part part)
        {
            ToSequencer(part).Run(m_decisionMaker);
        }

        protected void RunUntil<TStep>(Part part)
            where TStep : Step
        {
            RunUntil(ToSequencer(part), m_decisionMaker, IsNotStep<TStep>);
        }

        /// <summary>
        /// Runs until a part fails the given <paramref name="test"/>.
        /// </summary>
        /// <returns></returns>
        private static bool RunUntil(Sequencer sequencer, IChoiceDecisionMaker controller, Func<Part, bool> test)
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

        private static bool IsNotStep<TStep>(Part part)
            where TStep : Step
        {
            SequenceStep sequence = part as SequenceStep;
            if (sequence != null)
            {
                return !(sequence.Step is TStep);
            }
            return true;
        }

        private Sequencer ToSequencer(Part part)
        {
            return new Sequencer(m_game, part);
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
