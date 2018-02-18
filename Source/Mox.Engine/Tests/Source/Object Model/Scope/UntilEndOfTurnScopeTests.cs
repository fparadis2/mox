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
using Mox.Events;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class UntilEndOfTurnScopeTests : BaseGameTests
    {
        #region Inner Types

        private class EmptyEffect : Effect<PowerAndToughness>
        {
            public EmptyEffect()
                : base(Card.PowerAndToughnessProperty)
            {
            }

            public override PowerAndToughness Modify(Object owner, PowerAndToughness value)
            {
                return value;
            }
        }

        #endregion

        #region Variables

        private LocalEffectInstance m_instance;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_instance = m_game.CreateScopedLocalEffect<UntilEndOfTurnScope>(m_card, new EmptyEffect());
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Scope_removes_effect_at_the_end_of_turn()
        {
            m_game.Events.Trigger(new EndOfTurnEvent(m_playerA));

            Assert.IsFalse(m_game.Objects.Contains(m_instance));
        }

        #endregion
    }
}
