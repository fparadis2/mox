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
    public class LogOriginTests
    {
        #region Variables

        private LogOrigin m_origin;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_origin = new LogOrigin();
        }

        #endregion

        #region Utility

        private void TestToString(string expected, string source, int line, int column, int endLine, int endColumn)
        {
            Assert.AreEqual(expected, new LogOrigin() { Source = source, Line = line, Column = column, EndLine = endLine, EndColumn = endColumn }.ToString());
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Source()
        {
            Assert.IsNullOrEmpty(m_origin.Source);
            m_origin.Source = "My Source";
            Assert.AreEqual("My Source", m_origin.Source);
        }

        [Test]
        public void Test_Line()
        {
            Assert.AreEqual(0, m_origin.Line);
            m_origin.Line = 3;
            Assert.AreEqual(3, m_origin.Line);
        }

        [Test]
        public void Test_Column()
        {
            Assert.AreEqual(0, m_origin.Column);
            m_origin.Column = 4;
            Assert.AreEqual(4, m_origin.Column);
        }

        [Test]
        public void Test_EndLine()
        {
            Assert.AreEqual(0, m_origin.EndLine);
            m_origin.EndLine = 5;
            Assert.AreEqual(5, m_origin.EndLine);
        }

        [Test]
        public void Test_EndColumn()
        {
            Assert.AreEqual(0, m_origin.EndColumn);
            m_origin.EndColumn = 6;
            Assert.AreEqual(6, m_origin.EndColumn);
        }

        [Test]
        public void Test_Empty_returns_an_empty_origin()
        {
            Assert.IsNullOrEmpty(LogOrigin.Empty.Source);
            Assert.AreEqual(0, LogOrigin.Empty.Line);
        }

        [Test]
        public void Test_ToString()
        {
            Assert.AreEqual(string.Empty, LogOrigin.Empty.ToString());
            TestToString(string.Empty, null, 1, 2, 3, 4); // No source = no string
            TestToString(string.Empty, string.Empty, 1, 2, 3, 4); // No source = no string

            TestToString("My Source",       "My Source", 0, 1, 2, 3);

            TestToString("My Source(1)",        "My Source", 1, 0, 0, 0);
            TestToString("My Source(1,2)",      "My Source", 1, 2, 0, 0);
            TestToString("My Source(1-2)",      "My Source", 1, 0, 2, 0);
            TestToString("My Source(1)",        "My Source", 1, 0, 0, 2);
            TestToString("My Source(1-3,2)",    "My Source", 1, 2, 3, 0);
            TestToString("My Source(1-2)",      "My Source", 1, 0, 2, 3);
            TestToString("My Source(1,2-3)",    "My Source", 1, 2, 0, 3);
            TestToString("My Source(1,2,3,4)",  "My Source", 1, 2, 3, 4);
        }

        #endregion
    }
}
