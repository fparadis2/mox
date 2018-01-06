﻿// Copyright (c) François Paradis
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

namespace Mox.Abilities
{
    [TestFixture]
    public class ContinuousAbilityTests : BaseGameTests
    {
        #region Mock Types

        private class MockContinuousAbility : ContinuousAbility
        {
            protected override IEnumerable<IEffectCreator> AddEffects()
            {
                yield return AddEffect.On(Source).ModifyPowerAndToughness(+1, 0);
            }
        }

        #endregion

        #region Variables

        private MockContinuousAbility m_ability;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_ability = m_game.CreateAbility<MockContinuousAbility>(m_card);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Ability_only_has_an_effect_when_card_is_in_play()
        {
            m_card.Zone = m_game.Zones.Hand;
            Assert.AreEqual(0, m_card.Power);

            m_card.Zone = m_game.Zones.Battlefield;
            Assert.AreEqual(1, m_card.Power);

            m_card.Zone = m_game.Zones.Hand;
            Assert.AreEqual(0, m_card.Power);
        }

        #endregion
    }
}
