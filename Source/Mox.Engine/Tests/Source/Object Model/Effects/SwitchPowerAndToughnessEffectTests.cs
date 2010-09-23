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

namespace Mox.Effects
{
    [TestFixture]
    public class SwitchPowerAndToughnessEffectTests : BaseGameTests
    {
        #region Variables

        private SwitchPowerAndToughnessEffect m_effect;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();
            m_effect = new SwitchPowerAndToughnessEffect();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(EffectDependencyLayer.PT_Switch, m_effect.DependendencyLayer);
        }

        [Test]
        public void Test_Is_Serializable()
        {
            SwitchPowerAndToughnessEffect effect = Assert.IsSerializable(m_effect);
            Assert.IsNotNull(effect);

            PowerAndToughness pt = new PowerAndToughness { Power = 3, Toughness = 10 };
            pt = effect.Modify(m_card, pt);

            Assert.AreEqual(10, pt.Power);
            Assert.AreEqual(3, pt.Toughness);
        }

        [Test]
        public void Test_Modify_is_simple_arithmetics()
        {
            PowerAndToughness pt = new PowerAndToughness { Power = 3, Toughness = 10 };
            pt = m_effect.Modify(m_card, pt);

            Assert.AreEqual(10, pt.Power);
            Assert.AreEqual(3, pt.Toughness);
        }

        #endregion
    }
}
