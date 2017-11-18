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

namespace Mox
{
    [TestFixture]
    public class EventSinkTests
    {
        #region Inner Types

        private class MyEventArgs : EventArgs
        {
        }

        private class EventSource
        {
            public bool NoEventCalledYet = true;

            public event EventHandler EventWithoutArgs;

            public void TriggerEventWithoutArgs(object sender, EventArgs e)
            {
                if (EventWithoutArgs != null)
                {
                    EventWithoutArgs(sender, e);
                }

                NoEventCalledYet = false;
            }

            public event EventHandler<MyEventArgs> EventWithArgs;

            public void TriggerEventWithArgs(object sender, MyEventArgs e)
            {
                if (EventWithArgs != null)
                {
                    EventWithArgs(sender, e);
                }

                NoEventCalledYet = false;
            }
        }

        #endregion

        #region Variables

        private EventSource m_source;
        private EventSink m_sinkWithoutArgs;
        private EventSink<MyEventArgs> m_sinkWithArgs;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_source = new EventSource();

            m_sinkWithoutArgs = new EventSink();
            m_sinkWithArgs = new EventSink<MyEventArgs>(null);

            m_source.EventWithoutArgs += m_sinkWithoutArgs;
            m_source.EventWithArgs += m_sinkWithArgs;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_TimesCalled_is_incremented_each_time_the_event_is_triggered()
        {
            Assert.AreEqual(0, m_sinkWithoutArgs.TimesCalled);
            m_source.TriggerEventWithoutArgs(null, EventArgs.Empty);
            Assert.AreEqual(1, m_sinkWithoutArgs.TimesCalled);
            m_source.TriggerEventWithoutArgs(null, EventArgs.Empty);
            Assert.AreEqual(2, m_sinkWithoutArgs.TimesCalled);
        }

        [Test]
        public void Test_TimesCalled_is_incremented_each_time_the_event_is_triggered_with_args()
        {
            Assert.AreEqual(0, m_sinkWithArgs.TimesCalled);
            m_source.TriggerEventWithArgs(null, new MyEventArgs());
            Assert.AreEqual(1, m_sinkWithArgs.TimesCalled);
            m_source.TriggerEventWithArgs(null, new MyEventArgs());
            Assert.AreEqual(2, m_sinkWithArgs.TimesCalled);
        }

        [Test]
        public void Test_LastSender_retains_the_sender_of_the_last_handling()
        {
            Assert.IsNull(m_sinkWithoutArgs.LastSender);

            object sender = new object();
            m_source.TriggerEventWithoutArgs(sender, EventArgs.Empty);
            Assert.AreEqual(sender, m_sinkWithoutArgs.LastSender);

            sender = new object();
            m_source.TriggerEventWithoutArgs(sender, EventArgs.Empty);
            Assert.AreEqual(sender, m_sinkWithoutArgs.LastSender);
        }

        [Test]
        public void Test_LastSender_retains_the_sender_of_the_last_handling_with_args()
        {
            Assert.IsNull(m_sinkWithArgs.LastSender);

            object sender = new object();
            m_source.TriggerEventWithArgs(sender, new MyEventArgs());
            Assert.AreEqual(sender, m_sinkWithArgs.LastSender);

            sender = new object();
            m_source.TriggerEventWithArgs(sender, new MyEventArgs());
            Assert.AreEqual(sender, m_sinkWithArgs.LastSender);
        }

        [Test]
        public void Test_LastEventArgs_retains_the_args_of_the_last_handling()
        {
            Assert.IsNull(m_sinkWithoutArgs.LastEventArgs);

            EventArgs args = new EventArgs();
            m_source.TriggerEventWithoutArgs(null, args);
            Assert.AreEqual(args, m_sinkWithoutArgs.LastEventArgs);

            args = new EventArgs();
            m_source.TriggerEventWithoutArgs(null, args);
            Assert.AreEqual(args, m_sinkWithoutArgs.LastEventArgs);
        }

        [Test]
        public void Test_LastEventArgs_retains_the_args_of_the_last_handling_with_args()
        {
            Assert.IsNull(m_sinkWithArgs.LastEventArgs);

            MyEventArgs args = new MyEventArgs();
            m_source.TriggerEventWithArgs(null, args);
            Assert.AreEqual(args, m_sinkWithArgs.LastEventArgs);

            args = new MyEventArgs();
            m_source.TriggerEventWithArgs(null, args);
            Assert.AreEqual(args, m_sinkWithArgs.LastEventArgs);
        }

        [Test]
        public void Test_Can_set_an_expected_sender()
        {
            object expectedSender = new object();

            EventSink sink = new EventSink(expectedSender);
            m_source.EventWithoutArgs += sink;

            Assert.DoesntThrow(delegate { m_source.TriggerEventWithoutArgs(expectedSender, EventArgs.Empty); });
            Assert.Throws<AssertionException>(delegate { m_source.TriggerEventWithoutArgs(null, EventArgs.Empty); });
            Assert.Throws<AssertionException>(delegate { m_source.TriggerEventWithoutArgs(new object(), EventArgs.Empty); });
        }

        [Test]
        public void Test_Callback_is_triggered_during_the_event()
        {
            object callbackSender;
            MyEventArgs callbackArgs;

            m_sinkWithArgs.Callback += (sender, args) => 
            { 
                callbackSender = sender; 
                callbackArgs = args; 
                Assert.IsTrue(m_source.NoEventCalledYet); 
            };

            object realSender = new object();
            MyEventArgs realArgs = new MyEventArgs();
            m_source.TriggerEventWithArgs(realSender, realArgs);
        }

        [Test]
        public void Test_AssertEventNotCalled()
        {
            Assert.DoesntThrow(delegate { Assert.EventNotCalled(m_sinkWithoutArgs, delegate { }); });
            Assert.Throws<AssertionException>(delegate { Assert.EventNotCalled(m_sinkWithoutArgs, delegate { m_source.TriggerEventWithoutArgs(null, EventArgs.Empty); }); });
        }

        [Test]
        public void Test_AssertEventCalledOnce()
        {
            Assert.DoesntThrow(delegate { Assert.EventCalledOnce(m_sinkWithoutArgs, delegate { m_source.TriggerEventWithoutArgs(null, EventArgs.Empty); }); });
            Assert.Throws<AssertionException>(delegate { Assert.EventCalledOnce(m_sinkWithoutArgs, delegate { m_source.TriggerEventWithoutArgs(null, EventArgs.Empty); m_source.TriggerEventWithoutArgs(null, EventArgs.Empty); }); });
            Assert.Throws<AssertionException>(delegate { Assert.EventCalledOnce(m_sinkWithoutArgs, delegate { }); });
        }

        [Test]
        public void Test_AssertEventCalled()
        {
            Assert.DoesntThrow(delegate { Assert.EventCalled(m_sinkWithoutArgs, delegate { m_source.TriggerEventWithoutArgs(null, EventArgs.Empty); }); });
            Assert.DoesntThrow(delegate { Assert.EventCalled(m_sinkWithoutArgs, delegate { m_source.TriggerEventWithoutArgs(null, EventArgs.Empty); m_source.TriggerEventWithoutArgs(null, EventArgs.Empty); }); });
            Assert.Throws<AssertionException>(delegate { Assert.EventCalled(m_sinkWithoutArgs, delegate { }); });
        }

        [Test]
        public void Test_AssertEventCalled_exactly()
        {
            Assert.DoesntThrow(delegate { Assert.EventCalled(m_sinkWithoutArgs, delegate { m_source.TriggerEventWithoutArgs(null, EventArgs.Empty); }, 1); });
            Assert.DoesntThrow(delegate { Assert.EventCalled(m_sinkWithoutArgs, delegate { m_source.TriggerEventWithoutArgs(null, EventArgs.Empty); m_source.TriggerEventWithoutArgs(null, EventArgs.Empty); }, 2); });
            Assert.DoesntThrow(delegate { Assert.EventCalled(m_sinkWithoutArgs, delegate { }, 0); });

            Assert.Throws<AssertionException>(delegate { Assert.EventCalled(m_sinkWithoutArgs, delegate { }, 1); });
            Assert.Throws<AssertionException>(delegate { Assert.EventCalled(m_sinkWithoutArgs, delegate { m_source.TriggerEventWithoutArgs(null, EventArgs.Empty); }, 2); });
        }

        #endregion
    }
}
