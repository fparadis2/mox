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

using Mox.Abilities;

namespace Mox.Flow.Parts
{
    [TestFixture]
    public class PlayUntilAllPlayersPassAndTheStackIsEmptyTests : PartTestBase
    {
        #region Variables

        private PlayUntilAllPlayersPassAndTheStackIsEmpty m_part;
        private MockAbility m_mockAbilityB;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_part = new PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            Card cardB = CreateCard(m_playerB);
            m_mockAbilityB = CreateMockAbility(cardB, AbilityType.Normal);
        }

        #endregion

        #region Utilities

        private void Run()
        {
            m_sequencerTester.Run(m_part);
        }

        private static void Expect_Play_Mock_Ability(MockAbility mockAbility)
        {
            mockAbility.Expect_Play().Repeat.Twice();
        }

        private void Expect_Play_Spell_that_doesnt_use_stack(MockAbility ability)
        {
            ISpellEffect spellEffect = m_mockery.StrictMock<ISpellEffect>();

            ability.Expect_Play(spell =>
            {
                spell.UseStack = false;
                spell.Effect = s => spellEffect.Do();
            }).Repeat.Twice();

            spellEffect.Do();
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
            using (OrderedExpectations)
            {
                m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(m_mockAbility));
                Expect_Play_Mock_Ability(m_mockAbility);

                m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

                // Spell resolves here

                // Both players get another chance to play after the spell resolves
                m_sequencerTester.Expect_Everyone_passes_once(m_playerA);
            }

            Run();
        }

        [Test]
        public void Test_ActivePlayer_gets_priority_after_a_spell_resolves_even_if_another_player_played_before()
        {
            using (OrderedExpectations)
            {
                m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);
                m_sequencerTester.Expect_Player_GivePriority(m_playerB, new Mox.PlayAbility(m_mockAbilityB));
                Expect_Play_Mock_Ability(m_mockAbilityB);

                m_sequencerTester.Expect_Everyone_passes_once(m_playerB);

                // Spell resolves here

                // A gets priority once again after the spell resolves.
                m_sequencerTester.Expect_Everyone_passes_once(m_playerA);
            }

            Run();
        }

        [Test]
        public void Test_Spells_are_resolved_when_popped_from_the_stack()
        {
            using (OrderedExpectations)
            {
                m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(m_mockAbility));
                ISpellEffect spellEffect = Expect_Play_Ability(m_mockAbility, m_playerA);

                m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

                // Spell resolves here
                spellEffect.Do();

                // A gets priority once again after the spell resolves.
                m_sequencerTester.Expect_Everyone_passes_once(m_playerA);
            }

            Run();
        }

        [Test]
        public void Test_Sequencing_stops_if_the_game_ends_after_a_spell_has_resolved()
        {
            using (OrderedExpectations)
            {
                m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(m_mockAbility));
                ISpellEffect spellEffect = Expect_Play_Ability(m_mockAbility, m_playerA);

                m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

                // Spell resolves here
                spellEffect.Do();

                // Game ends
                LastCall.Callback(() =>
                {
                    m_game.State.Winner = m_playerA;
                    return true;
                });
            }

            Run();
        }

        [Test]
        public void Test_Spells_are_resolved_in_the_inverse_order_of_when_they_were_pushed()
        {
            using (OrderedExpectations)
            {
                m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(m_mockAbility));
                ISpellEffect spellEffect1 = Expect_Play_Ability(m_mockAbility, m_playerA);

                m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(m_mockAbility));
                ISpellEffect spellEffect2 = Expect_Play_Ability(m_mockAbility, m_playerA);

                m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);

                m_sequencerTester.Expect_Player_GivePriority(m_playerB, new Mox.PlayAbility(m_mockAbilityB));
                ISpellEffect spellEffect3 = Expect_Play_Ability(m_mockAbilityB, m_playerB);

                m_sequencerTester.Expect_Everyone_passes_once(m_playerB);

                spellEffect3.Do();
                m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

                spellEffect2.Do();
                m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

                spellEffect1.Do();
                m_sequencerTester.Expect_Everyone_passes_once(m_playerA);
            }

            Run();
        }

        [Test]
        public void Test_Sequencing_stops_when_game_ends_even_with_spells_still_on_the_stack()
        {
            using (OrderedExpectations)
            {
                m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(m_mockAbility));
                Expect_Play_Ability(m_mockAbility, m_playerA);

                m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(m_mockAbility));
                Expect_Play_Ability(m_mockAbility, m_playerA);

                m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);

                m_sequencerTester.Expect_Player_GivePriority(m_playerB, new Mox.PlayAbility(m_mockAbilityB));
                ISpellEffect spellEffect3 = Expect_Play_Ability(m_mockAbilityB, m_playerB);

                m_sequencerTester.Expect_Everyone_passes_once(m_playerB);

                spellEffect3.Do();

                // Game ends
                LastCall.Callback(() =>
                {
                    m_game.State.Winner = m_playerA;
                    return true;
                });
            }

            Run();
        }

        [Test]
        public void Test_Spells_can_be_pushed_right_after_others_have_resolved()
        {
            using (OrderedExpectations)
            {
                m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(m_mockAbility));
                ISpellEffect spellEffect1 = Expect_Play_Ability(m_mockAbility, m_playerA);

                m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(m_mockAbility));
                ISpellEffect spellEffect2 = Expect_Play_Ability(m_mockAbility, m_playerA);

                m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

                spellEffect2.Do();

                m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(m_mockAbility));
                ISpellEffect spellEffect3 = Expect_Play_Ability(m_mockAbility, m_playerA);
                m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

                spellEffect3.Do();
                m_sequencerTester.Expect_Everyone_passes_once(m_playerA);

                spellEffect1.Do();
                m_sequencerTester.Expect_Everyone_passes_once(m_playerA);
            }

            Run();
        }

        [Test]
        public void Test_Spells_that_do_not_use_the_stack_are_resolved_immediatly()
        {
            using (OrderedExpectations)
            {
                m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(m_mockAbility));
                Expect_Play_Spell_that_doesnt_use_stack(m_mockAbility);

                m_sequencerTester.Expect_Player_GivePriority(m_playerA, new Mox.PlayAbility(m_mockAbility));
                Expect_Play_Spell_that_doesnt_use_stack(m_mockAbility);

                m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);

                m_sequencerTester.Expect_Player_GivePriority(m_playerB, new Mox.PlayAbility(m_mockAbilityB));
                Expect_Play_Spell_that_doesnt_use_stack(m_mockAbilityB);

                m_sequencerTester.Expect_Everyone_passes_once(m_playerB);
            }

            Run();
        }

        #endregion
    }
}
