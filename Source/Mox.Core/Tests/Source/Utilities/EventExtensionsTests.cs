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
using System.Text;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class EventExtensionsTests
    {
        #region Inner Types

        private class MyEventArgs : EventArgs
        {
        }

        #endregion

        #region Events

        private event EventHandler ArglessEvent;
        private event EventHandler<MyEventArgs> NormalEvent;

        #endregion

        #region Tests

        [Test]
        public void Test_Raise_raises_the_event_with_argless_event()
        {
            EventSink sink = new EventSink(this);
            ArglessEvent += sink;

            MyEventArgs e = new MyEventArgs();

            Assert.EventCalledOnce(sink, () => ArglessEvent.Raise(this, e));
            Assert.AreSame(e, sink.LastEventArgs);

            ArglessEvent -= sink;
        }

        [Test]
        public void Test_Raise_raises_the_event_with_normal_event()
        {
            EventSink<MyEventArgs> sink = new EventSink<MyEventArgs>(this);
            NormalEvent += sink;

            MyEventArgs e = new MyEventArgs();

            Assert.EventCalledOnce(sink, () => NormalEvent.Raise(this, e));
            Assert.AreSame(e, sink.LastEventArgs);

            NormalEvent -= sink;
        }

        [Test]
        public void Test_Raise_doesnt_fail_if_event_is_null()
        {
            Assert.IsNull(ArglessEvent, "Sanity check");
            Assert.IsNull(NormalEvent, "Sanity check");

            ArglessEvent.Raise(this, new MyEventArgs());
            NormalEvent.Raise(this, new MyEventArgs());
        }

        #endregion
    }
}
