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

using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.AI
{
    /// <summary>
    /// Those are tests that I would expect to hold for any well-formed algorithm.
    /// </summary>
    public abstract class WellFormedAlgorithmBaseTests : BaseGameTests
    {
        #region Variables

        protected IMinMaxAlgorithm m_algorithm;
        protected IMinimaxTree m_tree;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_tree = m_mockery.StrictMock<IMinimaxTree>();
        }

        #endregion

        #region Utilities

        protected float ComputeHeuristic()
        {
            return ComputeHeuristic(true);
        }

        protected float ComputeHeuristic(bool considerGameEndingState)
        {
            return m_algorithm.ComputeHeuristic(m_game, considerGameEndingState);
        }

        #endregion

        #region Tests

        #region Assumptions

        [Test]
        public void Test_These_tests_are_based_around_the_fact_that_player_A_is_maximizing()
        {
            Assert.That( m_algorithm.IsMaximizingPlayer(m_playerA), "These tests assume that player A is a maximizing player.");
            Assert.That(!m_algorithm.IsMaximizingPlayer(m_playerB), "These tests assume that player B is a minimizing player.");
        }

        #endregion

        #region IsTerminal

        [Test]
        public void Test_IsTerminal_always_returns_true_if_the_game_has_ended()
        {
            m_game.State.Winner = m_playerA;
            Assert.IsTrue(m_algorithm.IsTerminal(m_tree, m_game));

            m_game.State.Winner = m_playerB;
            Assert.IsTrue(m_algorithm.IsTerminal(m_tree, m_game));
        }

        #endregion

        #region Heuristic computation tests

        [Test]
        public void Test_Game_value_is_very_large_if_maximizing_player_is_a_winner()
        {
            m_game.State.Winner = m_playerA;
            Assert.AreEqual(BaseMinMaxAlgorithm.MaxValue, ComputeHeuristic());
            Assert.Greater(BaseMinMaxAlgorithm.MaxValue, ComputeHeuristic(false));
        }

        [Test]
        public void Test_Game_value_is_very_small_if_maximizing_player_is_a_loser()
        {
            m_game.State.Winner = m_playerB;
            Assert.AreEqual(BaseMinMaxAlgorithm.MinValue, ComputeHeuristic());
            Assert.Less(BaseMinMaxAlgorithm.MinValue, ComputeHeuristic(false));
        }

        [Test]
        public void Test_Game_value_is_proportional_to_maximizing_player_life()
        {
            m_playerA.Life = 10;
            float originalValue = ComputeHeuristic();

            m_playerA.Life = 20;
            Assert.Greater(ComputeHeuristic(), originalValue);

            m_playerA.Life = 0;
            Assert.Less(ComputeHeuristic(), originalValue);
        }

        [Test]
        public void Test_Game_value_is_inversely_proportional_to_minimizing_player_life()
        {
            m_playerB.Life = 10;
            float originalValue = ComputeHeuristic();

            m_playerB.Life = 20;
            Assert.Less(ComputeHeuristic(), originalValue);

            m_playerB.Life = 0;
            Assert.Greater(ComputeHeuristic(), originalValue);
        }

        [Test]
        public void Test_Heuristic_usually_doesnt_depend_on_game_ending_state()
        {
            m_playerB.Life = 10;
            Assert.AreEqual(ComputeHeuristic(), ComputeHeuristic(false));

            m_playerB.Life = 20;
            Assert.AreEqual(ComputeHeuristic(), ComputeHeuristic(false));

            m_playerB.Life = 0;
            Assert.AreEqual(ComputeHeuristic(), ComputeHeuristic(false));
        }

        #endregion

        #endregion
    }
}
