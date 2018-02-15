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
    public class ChangeControllerEffectTests : BaseGameTests
    {
        #region Variables

        private ChangeControllerEffect m_effect;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_effect = new ChangeControllerEffect(m_playerA);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_playerA, m_effect.Controller.Resolve(m_game));
            Assert.AreEqual(EffectDependencyLayer.ControlChanging, m_effect.DependendencyLayer);
        }

        [Test]
        public void Test_Is_Serializable()
        {
            ChangeControllerEffect effect = Assert.IsSerializable(m_effect);
            Assert.IsNotNull(effect);

            Assert.AreEqual(m_playerA, effect.Controller.Resolve(m_game));
        }

        [Test]
        public void Test_Modify_returns_the_new_controller()
        {
            Assert.AreEqual(m_playerA, m_effect.Modify(m_card, m_playerB));
        }

        #endregion
    }
}
