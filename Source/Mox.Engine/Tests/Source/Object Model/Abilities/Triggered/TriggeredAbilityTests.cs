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

namespace Mox.Abilities
{
    [TestFixture]
    public class TriggeredAbilityTests : BaseGameTests
    {
        #region Mock Types

        private class Patate : Event
        {
        }

        private class PatateTrigger : Trigger
        {
            public override IEnumerable<System.Type> EventTypes
            {
                get { yield return typeof(Patate); }
            }

            public override bool ShouldTrigger(TriggeredAbility2 ability, Event e)
            {
                Assert.IsInstanceOf<Patate>(e);
                return true;
            }
        }

        #endregion

        #region Variables

        private TriggeredAbility2 m_ability;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            var spellDefinition = CreateSpellDefinition(m_card);
            spellDefinition.Trigger = new PatateTrigger();

            m_ability = m_game.CreateAbility<TriggeredAbility2>(m_card, spellDefinition);
            m_card.Zone = m_game.Zones.Battlefield;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Triggered_abilities_are_of_type_trigger()
        {
            Assert.AreEqual(AbilityType.Triggered, m_ability.AbilityType);
        }

        [Test]
        public void Test_Triggered_abilities_are_triggered_by_the_right_event()
        {
            Patate e = new Patate();
            m_game.Events.Trigger(e);

            Assert.AreEqual(1, m_game.GlobalData.TriggeredAbilities.Count());
            QueuedTriggeredAbility queuedAbility = m_game.GlobalData.TriggeredAbilities.First();
            Assert.AreEqual(m_ability, queuedAbility.Ability.Resolve(m_game));
        }

        [Test]
        public void Test_Triggered_abilities_do_nothing_when_not_visible()
        {
            m_card.Zone = m_game.Zones.Library;

            Patate e = new Patate();
            m_game.Events.Trigger(e);

            Assert.Collections.IsEmpty(m_game.GlobalData.TriggeredAbilities);
        }

        [Test]
        public void Test_Triggered_abilities_do_nothing_when_removed_from_the_game()
        {
            m_game.Objects.Remove(m_ability);

            Patate e = new Patate();
            m_game.Events.Trigger(e);

            Assert.Collections.IsEmpty(m_game.GlobalData.TriggeredAbilities);
        }

        #endregion
    }
}
