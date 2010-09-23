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
    [TestFixture]
    public class DefaultMinMaxAlgorithmTests : WellFormedAlgorithmBaseTests
    {
        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_algorithm = new DefaultMinMaxAlgorithm(m_playerA, new AIParameters { MinimumTreeDepth = 3 });
        }

        #endregion

        #region Utilities

        private float GetHeuristicForLife(int life)
        {
            m_playerA.Life = life;
            return ComputeHeuristic();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_arguments()
        {
            Assert.Throws<ArgumentNullException>(delegate { new DefaultMinMaxAlgorithm(m_playerA, null); });
        }

        [Test]
        public void Test_IsTerminal_returns_true_after_a_specific_number_of_plies()
        {
            Expect.Call(m_tree.Depth).Return(2);
            m_mockery.Test(() => Assert.IsFalse(m_algorithm.IsTerminal(m_tree, m_game)));

            Expect.Call(m_tree.Depth).Return(3);
            m_mockery.Test(() => Assert.IsTrue(m_algorithm.IsTerminal(m_tree, m_game)));

            Expect.Call(m_tree.Depth).Return(4);
            m_mockery.Test(() => Assert.IsTrue(m_algorithm.IsTerminal(m_tree, m_game)));
        }

        [Test]
        public void Test_IsTerminal_returns_false_if_the_spell_stack_is_not_empty()
        {
            m_mockery.Test(() =>
            {
                m_game.SpellStack.Push(new Spell(m_game, m_mockAbility, m_playerA));
                Assert.IsFalse(m_algorithm.IsTerminal(m_tree, m_game));
            });
        }

        [Test]
        public void Test_Cannot_end_the_search_while_in_combat()
        {
            m_mockery.Test(() =>
            {
                m_game.State.CurrentStep = Steps.DeclareAttackers;
                Assert.IsFalse(m_algorithm.IsTerminal(m_tree, m_game));

                m_game.State.CurrentStep = Steps.DeclareBlockers;
                Assert.IsFalse(m_algorithm.IsTerminal(m_tree, m_game));

                m_game.State.CurrentStep = Steps.CombatDamage;
                Assert.IsFalse(m_algorithm.IsTerminal(m_tree, m_game));
            });
        }

        #region Heuristic

        [Test]
        public void Test_Game_value_is_proportional_to_number_of_cards_in_play()
        {
            Card cardA = CreateCard(m_playerA); cardA.Zone = m_game.Zones.Hand;
            Card cardB = CreateCard(m_playerB); cardB.Zone = m_game.Zones.Hand;
            Card cardC = CreateCard(m_playerB); cardC.Zone = m_game.Zones.Hand;

            float originalValue = ComputeHeuristic();

            cardA.Zone = m_game.Zones.Battlefield;
            Assert.Greater(ComputeHeuristic(), originalValue);

            cardB.Zone = m_game.Zones.Battlefield;
            cardC.Zone = m_game.Zones.Battlefield;
            Assert.Less(ComputeHeuristic(), originalValue);
        }

        [Test]
        public void Test_Bigger_creatures_are_better()
        {
            Card cardA = CreateCard(m_playerA);
            cardA.Type = Type.Creature;
            cardA.Zone = m_game.Zones.Battlefield;
            
            cardA.Power = 1; cardA.Toughness = 1;
            float originalValue = ComputeHeuristic();

            cardA.Power = 1; cardA.Toughness = 10;
            Assert.Greater(ComputeHeuristic(), originalValue);
            originalValue = ComputeHeuristic();

            cardA.Power = 10; cardA.Toughness = 10;
            Assert.Greater(ComputeHeuristic(), originalValue);
        }

        private float GetLifeHeuristicDifference(int high, int low)
        {
            Assert.Greater(high, low);
            float diff = GetHeuristicForLife(high) - GetHeuristicForLife(low);
            Assert.Greater(diff, 0);
            return diff;
        }

        [Test]
        public void Test_Life_is_more_important_when_its_low()
        {
            float diff1 = GetLifeHeuristicDifference(1, 0);
            float diff2 = GetLifeHeuristicDifference(2, 1);
            float diff3 = GetLifeHeuristicDifference(10, 9);
            float diff4 = GetLifeHeuristicDifference(1000, 999);

            Assert.Greater(diff1, diff2);
            Assert.Greater(diff2, diff3);
            Assert.Greater(diff3, diff4);
        }

        #endregion

        #endregion
    }
}
