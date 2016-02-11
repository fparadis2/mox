using System;
using System.Globalization;
using NUnit.Framework;

namespace Mox.UI.Tests
{
    [TestFixture]
    public class DateTimeOffsetTests
    {
        #region Variables

        private DateTime m_now;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_now = DateTime.Now;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_ToString()
        {
            Assert.AreEqual("just now", new DateTimeOffset(m_now, m_now).ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual("just now", new DateTimeOffset(m_now, m_now.Subtract(TimeSpan.FromSeconds(30))).ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual("1 minute ago", new DateTimeOffset(m_now, m_now.Subtract(TimeSpan.FromSeconds(70))).ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual("2 minutes ago", new DateTimeOffset(m_now, m_now.Subtract(TimeSpan.FromSeconds(130))).ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual("32 minutes ago", new DateTimeOffset(m_now, m_now.Subtract(TimeSpan.FromSeconds(1930))).ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual("1 hour ago", new DateTimeOffset(m_now, m_now.Subtract(TimeSpan.FromMinutes(61))).ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual("2 hours ago", new DateTimeOffset(m_now, m_now.Subtract(TimeSpan.FromMinutes(121))).ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual("23 hours ago", new DateTimeOffset(m_now, m_now.Subtract(TimeSpan.FromHours(23))).ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual("yesterday", new DateTimeOffset(m_now, m_now.Subtract(TimeSpan.FromHours(25))).ToString(CultureInfo.InvariantCulture));

            DateTime then = m_now.Subtract(TimeSpan.FromDays(3));
            Assert.AreEqual(then.ToString("D", CultureInfo.InvariantCulture), new DateTimeOffset(m_now, then).ToString(CultureInfo.InvariantCulture));
        }

        #endregion
    }
}
