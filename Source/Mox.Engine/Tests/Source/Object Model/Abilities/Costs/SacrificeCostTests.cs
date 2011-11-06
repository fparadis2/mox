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
using Mox.Flow;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class TargetSacrificeCostTests : BaseGameTests
    {
        #region Variables

        private TargetSacrificeCost m_cost;
        private SequencerTester m_sequencer;

        private Predicate<ITargetable> m_predicate;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_predicate = (target => true);
            m_sequencer = new SequencerTester(m_mockery, m_game);
            m_sequencer.MockPlayerController(m_playerA);
            m_cost = new TargetSacrificeCost(Predicate);

            m_card.Zone = m_game.Zones.Battlefield;
        }

        #endregion

        #region Utilities

        private void Execute(Player player, bool expectedResult)
        {
            bool result = false;
            m_mockery.Test(() => result = m_cost.Execute(m_sequencer.Context, player));
            Assert.AreEqual(expectedResult, result);
        }

        private void Expect_Target(Player player, IEnumerable<ITargetable> possibleTargets, ITargetable result)
        {
            m_sequencer.Expect_Player_Target(player, true, possibleTargets, result, TargetContextType.Normal);
        }

        private bool Predicate(ITargetable targetable)
        {
            return m_predicate(targetable);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.That(m_cost.Result.IsEmpty);
        }

        [Test]
        public void Test_Invalid_Construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new TargetSacrificeCost(null); });
        }

        [Test]
        public void Test_Can_only_be_paid_if_there_exists_a_valid_target()
        {
            ExecutionEvaluationContext context = new ExecutionEvaluationContext();

            // Always allow to play.. can always cancel after that.
            Assert.IsTrue(m_cost.CanExecute(m_game, context));

            context.UserMode = true;

            // But in user mode, don't allow if no legal target
            m_cost = TargetCost.Creature().Sacrifice();
            m_card.Type = Type.Creature;
            m_card.Zone = m_game.Zones.Battlefield;
            Assert.IsTrue(m_cost.CanExecute(m_game, context));

            m_card.Type = Type.Artifact;
            Assert.IsFalse(m_cost.CanExecute(m_game, context));
        }

        [Test]
        public void Test_Execute()
        {
            Expect_Target(m_playerA, GetTargetables(m_cost.Filter), m_card);
            Execute(m_playerA, true);

            Assert.AreEqual(m_card, m_cost.Result.Resolve(m_game));
            Assert.AreEqual(m_game.Zones.Graveyard, m_card.Zone);
        }

        [Test]
        public void Test_Execute_will_ask_the_player_until_he_provides_a_valid_object()
        {
            m_predicate = (target => target is Card);

            Expect_Target(m_playerA, GetTargetables(m_cost.Filter), m_playerA);
            Expect_Target(m_playerA, GetTargetables(m_cost.Filter), m_card);
            Execute(m_playerA, true);

            Assert.AreEqual(m_card, m_cost.Result.Resolve(m_game));
            Assert.AreEqual(m_game.Zones.Graveyard, m_card.Zone);
        }

        [Test]
        public void Test_Execute_will_cancel_if_the_player_returns_nothing()
        {
            Expect_Target(m_playerA, GetTargetables(m_cost.Filter), null);
            Execute(m_playerA, false);
        }

        [Test]
        public void Test_Execute_will_cancel_if_there_are_no_valid_targets()
        {
            m_predicate = (target => false);
            Execute(m_playerA, false);
        }

        #endregion
    }

    [TestFixture]
    public class SacrificeCostTests : BaseGameTests
    {
        #region Variables

        private SacrificeCost m_cost;
        private SequencerTester m_sequencer;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_sequencer = new SequencerTester(m_mockery, m_game);

            m_cost = new SacrificeCost(m_card);
            m_card.Zone = m_game.Zones.Battlefield;
        }

        #endregion

        #region Utilities

        private void Execute(Player player, bool expectedResult)
        {
            bool result = false;
            m_mockery.Test(() => result = m_cost.Execute(m_sequencer.Context, player));
            Assert.AreEqual(expectedResult, result);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_Construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new SacrificeCost(null); });
        }

        [Test]
        public void Test_Can_only_be_paid_if_the_permanent_is_in_play()
        {
            ExecutionEvaluationContext context = new ExecutionEvaluationContext();


            m_card.Zone = m_game.Zones.Battlefield;
            Assert.IsTrue(m_cost.CanExecute(m_game, context));

            m_card.Zone = m_game.Zones.Graveyard;
            Assert.IsFalse(m_cost.CanExecute(m_game, context));
        }

        [Test]
        public void Test_Execute()
        {
            Execute(m_playerA, true);

            Assert.AreEqual(m_game.Zones.Graveyard, m_card.Zone);
        }

        #endregion
    }
}
