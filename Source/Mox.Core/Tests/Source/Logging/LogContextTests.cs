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
using System.Threading;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class LogContextTests
    {
        #region Variables

        private LogContext m_context;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_context = new LogContext();
        }

        #endregion

        #region Utilities

        private void Test_Log(LogMessage message)
        {
            m_context.Log(message);
            Assert.Collections.Contains(message, m_context.AllMessages);

            switch (message.Importance)
            {
                case LogImportance.Error:
                    Assert.Collections.Contains(message, m_context.Errors);
                    break;

                case LogImportance.Warning:
                    Assert.Collections.Contains(message, m_context.Warnings);
                    break;

                default:
                    Assert.Collections.Contains(message, m_context.Messages);
                    break;
            }
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Is_empty_by_default()
        {
            Assert.Collections.IsEmpty(m_context.AllMessages);
            Assert.Collections.IsEmpty(m_context.Errors);
            Assert.Collections.IsEmpty(m_context.Warnings);
            Assert.Collections.IsEmpty(m_context.Messages);
        }

        [Test]
        public void Test_All_collections_are_readonly_by_default()
        {
            Assert.IsTrue(m_context.AllMessages.IsReadOnly);
            Assert.IsTrue(m_context.Errors.IsReadOnly);
            Assert.IsTrue(m_context.Warnings.IsReadOnly);
            Assert.IsTrue(m_context.Messages.IsReadOnly);
        }

        [Test]
        public void Test_Log_adds_the_message_to_the_corresponding_collections()
        {
            Test_Log(new LogMessage() { Text = "MyMessage", Importance = LogImportance.Error });
            Test_Log(new LogMessage() { Text = "MyMessage", Importance = LogImportance.Warning });
            Test_Log(new LogMessage() { Text = "MyMessage", Importance = LogImportance.High });
        }

        #endregion
    }
}
