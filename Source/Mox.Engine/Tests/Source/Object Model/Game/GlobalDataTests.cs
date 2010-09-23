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
using System.Linq;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class GlobalDataTests : BaseGameTests
    {
        #region Variables

        private MockTriggeredAbility m_triggeredAbility;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_triggeredAbility = CreateMockTriggeredAbility(m_card);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_access_the_game_state_from_the_game()
        {
            Assert.IsNotNull(m_game.GlobalData);
            Assert.Collections.Contains(m_game.GlobalData, m_game.Objects);
        }

        [Test]
        public void Test_The_list_of_Triggered_abilities_is_empty_by_default()
        {
            Assert.Collections.IsEmpty(m_game.GlobalData.TriggeredAbilities);
        }

        [Test]
        public void Test_Trigger_adds_a_triggered_ability()
        {
            object context = new object();
            m_game.GlobalData.TriggerAbility(m_triggeredAbility, context);

            Assert.AreEqual(1, m_game.GlobalData.TriggeredAbilities.Count);
            QueuedTriggeredAbility qta = m_game.GlobalData.TriggeredAbilities.First();

            Assert.AreEqual(m_triggeredAbility, qta.Ability.Resolve(m_game));
            Assert.AreEqual(m_playerA, qta.Controller.Resolve(m_game));
            Assert.AreEqual(context, qta.Context);
        }

        #endregion
    }
}
