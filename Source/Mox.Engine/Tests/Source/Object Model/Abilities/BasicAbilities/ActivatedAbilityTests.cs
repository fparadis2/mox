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
    public class ActivatedAbilityTests : BaseGameTests
    {
        #region Variables

        private ActivatedAbility m_ability;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_ability = m_game.CreateAbility<ActivatedAbility>(m_card, SpellDefinition.Empty);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_only_play_when_the_source_is_in_play()
        {
            var context = new AbilityEvaluationContext(m_playerA, AbilityEvaluationContextType.Normal);

            m_card.Zone = m_game.Zones.Library;
            Assert.IsFalse(m_ability.CanPlay(context));

            m_card.Zone = m_game.Zones.Battlefield;
            Assert.IsTrue(m_ability.CanPlay(context));
        }

        #endregion
    }
}
