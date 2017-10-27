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
using System.Linq;

using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class PlayerManaPoolTests : BaseGameTests
    {
        #region Variables

        private PlayerManaPool m_manaPool;
        private EventSink<EventArgs> m_changedSink;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_manaPool = m_playerA.ManaPool;
            m_changedSink = new EventSink<EventArgs>(m_manaPool);
            m_manaPool.Changed += m_changedSink;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_construction_values()
        {
            Assert.AreEqual(m_playerA, m_manaPool.Player);
        }

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { m_manaPool = new PlayerManaPool(null); });
        }

        [Test]
        public void Test_Pool_is_empty_by_default()
        {
            foreach (Color color in Enum.GetValues(typeof(Color)))
            {
                Assert.AreEqual(0, m_manaPool[color]);
            }
        }

        [Test]
        public void Test_Cannot_use_a_mixed_color_with_the_mana_pool()
        {
            Assert.Throws<NotSupportedException>(delegate { int i = m_manaPool[Color.Red | Color.White]; });
        }

        [Test]
        public void Test_can_set_mana_in_the_pool()
        {
            foreach(Color color in Enum.GetValues(typeof(Color)))
            {
                m_manaPool[color] = 10;
                Assert.AreEqual(10, m_manaPool[color]);
            }
        }

        [Test]
        public void Test_ManaPool_operations_are_undoable()
        {
            Assert.IsUndoRedoable(m_game.Controller, 
                () => Assert.AreEqual(0, m_manaPool.Blue), 
                () => { m_manaPool.Blue = 3; }, 
                () => Assert.AreEqual(3, m_manaPool.Blue));
        }

        [Test]
        public void Test_Changed_is_triggered_when_mana_is_set_in_the_pool()
        {
            Assert.EventCalledOnce(m_changedSink, () => m_manaPool.Red = 10);
        }

        [Test]
        public void Test_Changed_is_not_triggered_when_no_mana_is_set_in_the_pool()
        {
            Assert.EventNotCalled(m_changedSink, () => m_manaPool.Red = 0);
        }

        #endregion
    }
}
