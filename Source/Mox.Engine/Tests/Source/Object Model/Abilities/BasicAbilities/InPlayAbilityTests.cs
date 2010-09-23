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

namespace Mox
{
    [TestFixture]
    public class InPlayAbilityTests : BaseGameTests
    {
        #region Inner Types

        private class MockInPlayAbility : InPlayAbility
        {
            #region Overrides of Ability

            /// <summary>
            /// Initializes the given spell and returns the "pre payment" costs associated with the spell (asks players for modal choices, {X} choices, etc...)
            /// </summary>
            /// <param name="spell"></param>
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                yield break;
            }

            #endregion
        }

        #endregion

        #region Variables

        private InPlayAbility m_ability;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_ability = m_game.CreateAbility<MockInPlayAbility>(m_card);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_only_play_when_the_source_is_in_play()
        {
            m_card.Zone = m_game.Zones.Library;
            Assert.IsFalse(m_ability.CanPlay(m_playerA, new ExecutionEvaluationContext()));

            m_card.Zone = m_game.Zones.Battlefield;
            Assert.IsTrue(m_ability.CanPlay(m_playerA, new ExecutionEvaluationContext()));
        }

        #endregion
    }
}
