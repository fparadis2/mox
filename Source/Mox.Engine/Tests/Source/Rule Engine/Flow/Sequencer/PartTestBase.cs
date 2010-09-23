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
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.Flow
{
    public abstract class PartTestBase<TPart> : PartTestUtilities
        where TPart : Part<IGameController>
    {
        #region Variables

        protected TPart m_part;

        #endregion

        #region Tests

        #endregion
    }

    public abstract class PartTestUtilities : BaseGameTests
    {
        #region Inner Types

        public interface ISpellEffect
        {
            void Do();
            void DoPre();
        }

        #endregion

        #region Variables

        protected IGameController m_controller;
        protected SequencerTester m_sequencerTester;

        protected Action m_mockAction;

        #endregion

        #region Setup

        public override void Setup()
        {
            base.Setup();

            m_sequencerTester = new SequencerTester(m_mockery, m_game);
            m_controller = m_sequencerTester.Controller;

            m_mockAction = m_mockery.StrictMock<Action>();

            m_sequencerTester.MockAllPlayers();
        }

        public override void Teardown()
        {
            Assert.That(m_sequencerTester.Sequencer.IsArgumentStackEmpty, "Argument stack should be empty after tests!");

            base.Teardown();
        }

        #endregion

        #region Utilities

        protected Part<TController> CreateMockPart<TController>()
        {
            return m_mockery.StrictMock<Part<TController>>();
        }

        protected Part<IGameController> Execute(Part<IGameController> part)
        {
            Part<IGameController> result = null;
            m_mockery.Test(() =>
            {
                result = part.Execute(m_sequencerTester.Context);
            });

            return result;
        }

        #region Expectations

        protected void Expect_Everyone_passes_once(Player startingPlayer)
        {
            foreach (Player player in Player.Enumerate(startingPlayer, false))
            {
                m_sequencerTester.Expect_Player_MockAction(player, null, new ExecutionEvaluationContext());
            }
        }

        protected void Expect_Player_Action(Player player, Action action)
        {
            m_sequencerTester.Expect_Player_Action(player, action);
        }

        protected void Expect_Player_MockAction(Player player, Action action)
        {
            m_sequencerTester.Expect_Player_MockAction(player, action, new ExecutionEvaluationContext());
        }

        protected void Expect_Player_Invalid_MockAction(Player player, Action action)
        {
            m_sequencerTester.Expect_Player_Invalid_MockAction(player, action, new ExecutionEvaluationContext());
        }

        protected ISpellEffect Expect_Play_Ability(MockAbility ability, Player player, params ImmediateCost[] costs)
        {
            ability.Expect_Play(costs, null);

            costs.ForEach(cost => Expect.Call(cost.CanExecute(m_game, new ExecutionEvaluationContext())).Return(true));

            return Expect_Play_Ability_Raw(ability, player, costs);
        }

        protected ISpellEffect Expect_Play_Ability_Raw(MockAbility ability, Player player, params ImmediateCost[] costs)
        {
            ISpellEffect spellEffect = m_mockery.StrictMock<ISpellEffect>();

            ability.Expect_Play_and_execute_costs(player, costs, null, spell =>
            {
                spell.PreEffect = (s, c) => spellEffect.DoPre();
                spell.Effect = (s, c) => spellEffect.Do();
            });

            spellEffect.DoPre();

            return spellEffect;
        }

        protected ISpellEffect Expect_Play_Ability_Delayed_Raw(MockAbility ability, Player player, params DelayedCost[] costs)
        {
            ISpellEffect spellEffect = m_mockery.StrictMock<ISpellEffect>();

            ability.Expect_Play_and_execute_costs(player, null, costs, spell =>
            {
                spell.PreEffect = (s, c) => spellEffect.DoPre();
                spell.Effect = (s, c) => spellEffect.Do();
            });

            spellEffect.DoPre();

            return spellEffect;
        }

        #endregion

        #endregion
    }
}