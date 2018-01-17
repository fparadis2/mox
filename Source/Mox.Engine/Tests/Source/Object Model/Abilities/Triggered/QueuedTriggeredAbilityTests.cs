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

namespace Mox.Abilities
{
    [TestFixture]
    public class QueuedTriggeredAbilityTests : BaseGameTests
    {
        #region Mock Types

        private class MockTriggeredAbility : TriggeredAbility
        {
        }

        #endregion

        #region Variables

        private readonly object m_context = new object();

        private MockTriggeredAbility m_ability;
        private QueuedTriggeredAbility m_queuedTriggeredAbility;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_ability = m_game.CreateAbility<MockTriggeredAbility>(m_card);
            m_queuedTriggeredAbility = new QueuedTriggeredAbility(m_ability, m_playerA, m_context);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_Construction_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => new QueuedTriggeredAbility(null, m_playerA, m_context));
            Assert.Throws<ArgumentNullException>(() => new QueuedTriggeredAbility(m_ability, null, m_context));
        }

        [Test]
        public void Test_Construction_arguments()
        {
            Assert.AreEqual(m_ability, m_queuedTriggeredAbility.Ability.Resolve(m_game));
            Assert.AreEqual(m_playerA, m_queuedTriggeredAbility.Controller.Resolve(m_game));
            Assert.AreEqual(m_context, m_queuedTriggeredAbility.Context);
        }

        #endregion
    }
}
