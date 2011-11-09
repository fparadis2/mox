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
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class TriggeredAbilityTests : BaseGameTests
    {
        #region Mock Types

        public class Patate
        {
        }

        private class MockTriggeredAbility : TriggeredAbility, IEventHandler<Patate>
        {
            public new void Trigger(object context)
            {
                base.Trigger(context);
            }

            public void HandleEvent(Game game, Patate e)
            {
                Implementation.HandleEvent(game, e);
            }

            public override void Play(Spell spell)
            {
            }

            internal IMockTriggeredAbilityImplementation Implementation
            {
                get;
                set;
            }
        }

        public interface IMockTriggeredAbilityImplementation
        {
            void HandleEvent(Game game, Patate e);
        }

        #endregion

        #region Variables

        private MockTriggeredAbility m_ability;
        private IMockTriggeredAbilityImplementation m_implementation;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_implementation = m_mockery.StrictMock<IMockTriggeredAbilityImplementation>();

            m_ability = m_game.CreateAbility<MockTriggeredAbility>(m_card);
            m_ability.Implementation = m_implementation;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Triggered_abilities_are_of_type_trigger()
        {
            Assert.AreEqual(AbilityType.Triggered, m_ability.AbilityType);
        }

        [Test]
        public void Test_Triggered_abilities_handlers_are_automatically_registered()
        {
            Patate e = new Patate();

            m_implementation.HandleEvent(m_game, e);

            m_mockery.Test(() => m_game.Events.Trigger(e));
        }

        [Test]
        public void Test_Triggered_abilities_handlers_are_automatically_unregistered_when_removed_from_the_game()
        {
            m_game.Objects.Remove(m_ability);

            Patate e = new Patate();
            m_mockery.Test(() => m_game.Events.Trigger(e));
        }

        [Test]
        public void Test_Trigger_adds_the_ability_to_the_list_of_triggered_abilities()
        {
            m_card.Zone = m_game.Zones.Battlefield;
            object context = new object();

            m_ability.Trigger(context);

            Assert.AreEqual(1, m_game.GlobalData.TriggeredAbilities.Count());
            QueuedTriggeredAbility queuedAbility = m_game.GlobalData.TriggeredAbilities.First();
            Assert.AreEqual(m_ability, queuedAbility.Ability.Resolve(m_game));
            Assert.AreEqual(context, queuedAbility.Context);
        }

        [Test]
        public void Test_Trigger_does_nothing_if_the_source_is_not_visible()
        {
            m_card.Zone = m_game.Zones.Library;
            m_ability.Trigger(null);
            Assert.Collections.IsEmpty(m_game.GlobalData.TriggeredAbilities);
        }

        #endregion
    }
}
