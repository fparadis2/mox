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
    public class LogMessageTests
    {
        #region Variables

        private LogMessage m_logMessage;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_logMessage = new LogMessage();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Text()
        {
            Assert.IsNullOrEmpty(m_logMessage.Text);
            m_logMessage.Text = "My Text";
            Assert.AreEqual("My Text", m_logMessage.Text);
        }

        [Test]
        public void Test_Importance()
        {
            Assert.AreEqual(LogImportance.Error, m_logMessage.Importance);
            m_logMessage.Importance = LogImportance.Normal;
            Assert.AreEqual(LogImportance.Normal, m_logMessage.Importance);
        }

        [Test]
        public void Test_SubCategory()
        {
            Assert.IsNullOrEmpty(m_logMessage.SubCategory);
            m_logMessage.SubCategory = "My Category";
            Assert.AreEqual("My Category", m_logMessage.SubCategory);
        }

        [Test]
        public void Test_Origin()
        {
            Assert.AreEqual(LogOrigin.Empty, m_logMessage.Origin);
            LogOrigin newOrigin = new LogOrigin() { Source = "My Source", Line = 3 };
            m_logMessage.Origin = newOrigin;
            Assert.AreEqual(newOrigin, m_logMessage.Origin);
        }

        [Test]
        public void Test_Code()
        {
            Assert.IsNullOrEmpty(m_logMessage.Code);
            m_logMessage.Code = "CODE";
            Assert.AreEqual("CODE", m_logMessage.Code);
        }

        [Test]
        public void Test_ToString()
        {
            Assert.AreEqual("error : ", new LogMessage() { Importance = LogImportance.Error }.ToString());

            Assert.AreEqual("error : My Message",   new LogMessage() { Importance = LogImportance.Error, Text = "My Message" }.ToString());
            Assert.AreEqual("warning : My Message", new LogMessage() { Importance = LogImportance.Warning, Text = "My Message" }.ToString());

            Assert.AreEqual("message : My Message", new LogMessage() { Importance = LogImportance.High, Text = "My Message" }.ToString());
            Assert.AreEqual("message : My Message", new LogMessage() { Importance = LogImportance.Normal, Text = "My Message" }.ToString());
            Assert.AreEqual("message : My Message", new LogMessage() { Importance = LogImportance.Low, Text = "My Message" }.ToString());
            Assert.AreEqual("message : My Message", new LogMessage() { Importance = LogImportance.Debug, Text = "My Message" }.ToString());

            Assert.AreEqual("error CODE: My Message",   new LogMessage() { Importance = LogImportance.Error, Text = "My Message", Code = "CODE" }.ToString());
            Assert.AreEqual("error CODE: ",             new LogMessage() { Importance = LogImportance.Error, Code = "CODE" }.ToString());

            Assert.AreEqual("sub category error CODE: My Message",  new LogMessage() { Importance = LogImportance.Error, Text = "My Message", Code = "CODE", SubCategory = "sub category" }.ToString());
            Assert.AreEqual("sub category error CODE: ",            new LogMessage() { Importance = LogImportance.Error, Code = "CODE", SubCategory = "sub category" }.ToString());

            LogOrigin origin = new LogOrigin() { Source = "My Source", Line = 3 };

            Assert.AreEqual("My Source(3): sub category error CODE: My Message",    new LogMessage() { Importance = LogImportance.Error, Text = "My Message", Code = "CODE", SubCategory = "sub category", Origin = origin }.ToString());
            Assert.AreEqual("My Source(3): sub category error CODE: ",              new LogMessage() { Importance = LogImportance.Error, Code = "CODE", SubCategory = "sub category", Origin = origin }.ToString());
            Assert.AreEqual("My Source(3): error : ",                               new LogMessage() { Importance = LogImportance.Error, Origin = origin }.ToString());
        }

        [Test]
        public void Test_ToString_with_multiline_text()
        {
            LogOrigin origin = new LogOrigin() { Source = "My Source", Line = 3 };
            Assert.AreEqual("My Source(3): error : First Line" + Environment.NewLine + "My Source(3): error : Second Line", new LogMessage() { Importance = LogImportance.Error, Text = "First Line\nSecond Line", Origin = origin }.ToString());
        }

        #endregion
    }
}
