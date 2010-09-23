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
    public class ChangeColorEffectTests : BaseGameTests
    {
        #region Variables

        private ChangeColorEffect m_effect;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_effect = new ChangeColorEffect(Color.Red);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(Color.Red, m_effect.Color);
            Assert.AreEqual(EffectDependencyLayer.ColorChanging, m_effect.DependendencyLayer);
        }

        [Test]
        public void Test_Is_Serializable()
        {
            ChangeColorEffect effect = Assert.IsSerializable(m_effect);
            Assert.IsNotNull(effect);

            Assert.AreEqual(Color.Red, effect.Color);
        }

        [Test]
        public void Test_Modify_returns_the_new_color()
        {
            Assert.AreEqual(Color.Red, m_effect.Modify(m_card, Color.Blue));
        }

        #endregion
    }
}
