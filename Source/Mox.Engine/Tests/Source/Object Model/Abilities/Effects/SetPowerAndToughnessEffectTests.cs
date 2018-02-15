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
    public class SetPowerAndToughnessEffectTests : BaseGameTests
    {
        #region Variables

        private ContinuousAbility m_ability;
        private SetPowerAndToughnessEffect m_effect;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_ability = m_game.CreateAbility<ContinuousAbility>(m_card);
            m_effect = new SetPowerAndToughnessEffect(m_ability, 1, 2);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(EffectDependencyLayer.PT_Set, m_effect.DependendencyLayer);
        }

        [Test]
        public void Test_Is_Serializable()
        {
            Assert.IsSerializable(m_effect);
        }

        [Test]
        public void Test_Modify_returns_the_new_value()
        {
            PowerAndToughness pt = new PowerAndToughness { Power = 3, Toughness = 10 };
            pt = m_effect.Modify(m_card, pt);

            Assert.AreEqual(1, pt.Power);
            Assert.AreEqual(2, pt.Toughness);
        }

        #endregion
    }
}
