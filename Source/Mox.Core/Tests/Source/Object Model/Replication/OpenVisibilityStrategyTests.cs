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

using NUnit.Framework;

namespace Mox.Replication
{
    [TestFixture]
    public class OpenVisibilityStrategyTests
    {
        #region Inner Types

        private class MyObject : Object
        {
            
        }

        #endregion

        #region Variables

        private OpenVisibilityStrategy<string> m_strategy;
        private Object m_object;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_strategy = new OpenVisibilityStrategy<string>();
            m_object = new MyObject();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_IsVisible_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => m_strategy.IsVisible(null, "Dummy"));
        }

        [Test]
        public void Test_Any_object_is_always_visible()
        {
            Assert.IsTrue(m_strategy.IsVisible(m_object, "Any"));
            Assert.IsTrue(m_strategy.IsVisible(m_object, "Thing"));
            Assert.IsTrue(m_strategy.IsVisible(m_object, null));
        }

        [Test]
        public void Test_Can_attach_and_detach_to_ObjectVisibilityChanged_event()
        {
            EventSink<VisibilityChangedEventArgs<string>> sink = new EventSink<VisibilityChangedEventArgs<string>>();
            m_strategy.ObjectVisibilityChanged += sink;
            m_strategy.ObjectVisibilityChanged -= sink;
        }

        #endregion
    }
}
