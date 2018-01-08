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
    public class StaticAbilityTests : BaseGameTests
    {
        #region Mock Types

        private class MockStaticAbility : StaticAbility
        {
        }

        #endregion

        #region Variables

        private MockStaticAbility m_ability;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_ability = m_game.CreateAbility<MockStaticAbility>(m_card);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Static_Abilities_are_of_type_Static()
        {
            Assert.AreEqual(AbilityType.Static, m_ability.AbilityType);
        }

        [Test]
        public void Test_Can_never_play_static_abilities()
        {
            var context = new AbilityEvaluationContext(m_playerA, AbilityEvaluationContextType.Normal);
            Assert.IsFalse(m_ability.CanPlay(context));
        }

        #endregion
    }
}
