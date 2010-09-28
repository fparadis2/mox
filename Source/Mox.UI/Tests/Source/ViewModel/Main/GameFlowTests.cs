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
using Rhino.Mocks;

namespace Mox.UI
{
    [TestFixture]
    public class GameFlowTests
    {
        #region Variables

        private IGameFlow m_defaultGameFlow;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_defaultGameFlow = new GameFlow.Default();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.IsNotNull(GameFlow.Instance);
        }

        [Test]
        public void Test_Can_Swap_instance()
        {
            MockRepository mockery = new MockRepository();
            var gameFlowInstance = mockery.StrictMock<IGameFlow>();

            var oldInstance = GameFlow.Instance;
            Assert.AreNotEqual(gameFlowInstance, oldInstance);

            using (GameFlow.Use(gameFlowInstance))
            {
                Assert.AreEqual(gameFlowInstance, GameFlow.Instance);
            }
            Assert.AreEqual(oldInstance, GameFlow.Instance);
        }

        #region Default game flow tests

        [Test]
        public void Test_Cannot_go_back_when_empty()
        {
            Assert.IsFalse(m_defaultGameFlow.CanGoBack);
            m_defaultGameFlow.PushPage<object>();
            m_defaultGameFlow.PushPage<object>();
            Assert.IsTrue(m_defaultGameFlow.CanGoBack);
        }

        [Test]
        public void Test_GoToPage_clears_the_stack()
        {
            m_defaultGameFlow.PushPage<object>();
            m_defaultGameFlow.PushPage<object>();
            m_defaultGameFlow.GoToPage<object>();
            Assert.IsFalse(m_defaultGameFlow.CanGoBack);
        }

        [Test]
        public void Test_GoToPage_sends_the_navigated_event()
        {
            EventSink<GameFlowNavigationEventArgs> sink = new EventSink<GameFlowNavigationEventArgs>(m_defaultGameFlow);

            m_defaultGameFlow.Navigated += sink;

            Assert.EventCalledOnce(sink, m_defaultGameFlow.GoToPage<GameFlowTests>);
            Assert.IsInstanceOf<GameFlowTests>(sink.LastEventArgs.Content);
        }

        [Test]
        public void Test_PushPage_sends_the_navigated_event()
        {
            EventSink<GameFlowNavigationEventArgs> sink = new EventSink<GameFlowNavigationEventArgs>(m_defaultGameFlow);

            m_defaultGameFlow.Navigated += sink;

            Assert.EventCalledOnce(sink, m_defaultGameFlow.PushPage<GameFlowTests>);
            Assert.IsInstanceOf<GameFlowTests>(sink.LastEventArgs.Content);
        }

        [Test]
        public void Test_GoBack_sends_the_navigated_event()
        {
            m_defaultGameFlow.PushPage<GameFlowTests>();
            m_defaultGameFlow.PushPage<object>();

            EventSink<GameFlowNavigationEventArgs> sink = new EventSink<GameFlowNavigationEventArgs>(m_defaultGameFlow);

            m_defaultGameFlow.Navigated += sink;

            Assert.EventCalledOnce(sink, m_defaultGameFlow.GoBack);
            Assert.IsInstanceOf<GameFlowTests>(sink.LastEventArgs.Content);
        }

        #endregion

        #endregion
    }
}
