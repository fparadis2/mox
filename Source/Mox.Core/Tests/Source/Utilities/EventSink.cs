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
    /// <summary>
    /// Allows the usage of EventSinks without the template arg.
    /// </summary>
    public interface IEventSink
    {
        int TimesCalled { get; }
    }

    /// <summary>
    /// Used to test events.
    /// </summary>
    public class EventSink<TEventArgs> : IEventSink
        where TEventArgs : EventArgs
    {
        #region Variables

        private readonly object m_expectedSender;

        private object m_lastSender;
        private TEventArgs m_lastEventArgs;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public EventSink()
            : this(null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="expectedSender"></param>
        public EventSink(object expectedSender)
        {
            m_expectedSender = expectedSender;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The number of times this sink has handled events.
        /// </summary>
        public int TimesCalled
        {
            get;
            set;
        }

        /// <summary>
        /// Last sender.
        /// </summary>
        public object LastSender
        {
            get { return m_lastSender; }
        }

        public TEventArgs LastEventArgs
        {
            get { return m_lastEventArgs; }
        }

        #endregion

        #region Methods

        public void Handler(object sender, TEventArgs e)
        {
            ++TimesCalled;
            m_lastSender = sender;
            m_lastEventArgs = e;

            if (m_expectedSender != null)
            {
                Assert.AreEqual(m_expectedSender, m_lastSender);
            }

            OnCallback(sender, e);
        }

        private void SimpleHandler(object sender, EventArgs e)
        {
            Handler(sender, e as TEventArgs);
        }

        #endregion

        #region Events

        /// <summary>
        /// Callback triggered during the event.
        /// </summary>
        public event EventHandler<TEventArgs> Callback;

        private void OnCallback(object sender, TEventArgs e)
        {
            if (Callback != null)
            {
                Callback(sender, e);
            }
        }

        #endregion

        #region Implicit Conversions

        /// <summary>
        /// Implicit conversion operator.
        /// </summary>
        /// <param name="sink"></param>
        /// <returns></returns>
        public static implicit operator EventHandler<TEventArgs>(EventSink<TEventArgs> sink)
        {
            return sink.Handler;
        }

        /// <summary>
        /// Implicit conversion operator.
        /// </summary>
        /// <param name="sink"></param>
        /// <returns></returns>
        public static implicit operator EventHandler(EventSink<TEventArgs> sink)
        {
            return sink.SimpleHandler;
        }

        #endregion
    }
}
