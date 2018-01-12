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

using NUnit.Framework;
using Rhino.Mocks;

using Mox.Abilities;

namespace Mox.Flow.Parts
{
    [TestFixture]
    public class PlayUntilAllPlayersPassAndTheStackIsEmptyTests : PartTestBase
    {
        #region Variables

        private PlayUntilAllPlayersPassAndTheStackIsEmpty m_part;

        private List<MockAction> m_actions;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_part = new PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);
            m_actions = new List<MockAction>();
        }

        #endregion

        #region Utilities

        private void Run()
        {
            m_sequencerTester.Run(m_part);
        }

        private List<MockAction> RecordResolveOrder()
        {
            List<MockAction> resolvedActions = new List<MockAction>();

            foreach (var action in m_actions)
            {
                action.Effect = () => resolvedActions.Add(action);
            }

            return resolvedActions;
        }

        private MockSpellAbility CreateAbility(Player player)
        {
            return CreateAbility(player, out MockAction dummy);
        }

        private MockSpellAbility CreateAbility(Player player, out MockAction action)
        {
            action = new MockAction();
            m_actions.Add(action);

            Card card = CreateCard(player);
            var ability = m_game.CreateAbility<MockSpellAbility>(card);
            ability.CreateSpellDefinition().AddAction(action);
            return ability;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_arguments()
        {
            Assert.Throws<ArgumentNullException>(delegate { new PlayUntilAllPlayersPassAndTheStackIsEmpty(null); });
        }

        [Test]
        public void Test_Construction_arguments()
        {
            Assert.AreEqual(m_playerA, m_part.GetPlayer(m_game));
        }

        [Test]
        public void Test_Player_that_pushed_spell_on_stack_gets_priority_after()
        {
            m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(CreateAbility(m_playerA)));
            m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

            // Spell resolves here

            // Both players get another chance to play after the spell resolves
            m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

            Run();
        }

        [Test]
        public void Test_ActivePlayer_gets_priority_after_a_spell_resolves_even_if_another_player_played_before()
        {
            m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);
            m_sequencerTester.Expect_Player_GivePriority(m_playerB, new Mox.PlayAbility(CreateAbility(m_playerB)));
            m_sequencerTester.Expect_Everyone_passes_once(m_playerB);

            // Spell resolves here

            // A gets priority once again after the spell resolves.
            m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

            Run();
        }

        [Test]
        public void Test_Spells_are_resolved_when_popped_from_the_stack()
        {
            var ability = CreateAbility(m_playerA, out MockAction action);

            m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(ability));
            m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

            // Spell resolves here

            // A gets priority once again after the spell resolves.
            m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

            Run();

            Assert.AreEqual(1, action.ResolveCount);
        }

        [Test]
        public void Test_Sequencing_stops_if_the_game_ends_after_a_spell_has_resolved()
        {
            var ability = CreateAbility(m_playerA, out MockAction action);

            action.Effect = () =>
            {
                m_game.State.Winner = m_playerA;
            };

            m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(ability));
            m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

            // Spell resolves here

            Run();
        }

        [Test]
        public void Test_Spells_are_resolved_in_the_inverse_order_of_when_they_were_pushed()
        {
            var ability1 = CreateAbility(m_playerA, out MockAction action1);
            var ability2 = CreateAbility(m_playerA, out MockAction action2);
            var ability3 = CreateAbility(m_playerB, out MockAction action3);

            m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(ability1));
            m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(ability2));
            m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);

            m_sequencerTester.Expect_Player_GivePriority(m_playerB, new Mox.PlayAbility(ability3));
            m_sequencerTester.Expect_Everyone_passes_once(m_playerB);

            // Spell 3 resolves here
            m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

            // Spell 2 resolves here
            m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

            // Spell 1 resolves here
            m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

            List<MockAction> resolvedActions = RecordResolveOrder();

            Run();

            Assert.Collections.AreEqual(new[] { action3, action2, action1 }, resolvedActions);
        }

        [Test]
        public void Test_Sequencing_stops_when_game_ends_even_with_spells_still_on_the_stack()
        {
            var ability = CreateAbility(m_playerB, out MockAction action);

            action.Effect = () =>
            {
                m_game.State.Winner = m_playerA;
            };

            m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(CreateAbility(m_playerA)));
            m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(CreateAbility(m_playerA)));
            m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);

            m_sequencerTester.Expect_Player_GivePriority(m_playerB, new Mox.PlayAbility(ability));
            m_sequencerTester.Expect_Everyone_passes_once(m_playerB);

            Run();
        }

        [Test]
        public void Test_Spells_can_be_pushed_right_after_others_have_resolved()
        {
            var ability1 = CreateAbility(m_playerA, out MockAction action1);
            var ability2 = CreateAbility(m_playerA, out MockAction action2);
            var ability3 = CreateAbility(m_playerA, out MockAction action3);

            m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(ability1));
            m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(ability2));
            m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

            // Spell 2 resolves here
            m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(ability3));
            m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

            // Spell 3 resolves here
            m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

            // Spell 1 resolves here
            m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

            List<MockAction> resolvedActions = RecordResolveOrder();

            Run();

            Assert.Collections.AreEqual(new[] { action2, action3, action1 }, resolvedActions);
        }

        [Test]
        public void Test_Spells_that_do_not_use_the_stack_are_resolved_immediatly()
        {
            var ability1 = CreateAbility(m_playerA, out MockAction action1);
            var ability2 = CreateAbility(m_playerA, out MockAction action2);
            var ability3 = CreateAbility(m_playerB, out MockAction action3);

            ability1.MockedUseStack = false;
            ability2.MockedUseStack = false;
            ability3.MockedUseStack = false;

            m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(ability1));
            m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(ability2));
            m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);

            m_sequencerTester.Expect_Player_GivePriority(m_playerB, new Mox.PlayAbility(ability3));
            m_sequencerTester.Expect_Everyone_passes_once(m_playerB);

            List<MockAction> resolvedActions = RecordResolveOrder();

            Run();

            Assert.Collections.AreEqual(new[] { action1, action2, action3 }, resolvedActions);
        }

        #endregion
    }
}
