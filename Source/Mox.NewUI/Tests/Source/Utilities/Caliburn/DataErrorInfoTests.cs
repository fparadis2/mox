using System;

using NUnit.Framework;

namespace Mox.UI
{
    [TestFixture]
    public class DataErrorInfoTests
    {
        #region Variables

        private DataErrorInfo m_dataError;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_dataError = new DataErrorInfo();
            MockProperty = 0;
        }

        #endregion

        #region Mock

        public int MockProperty
        {
            get;
            set;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.IsNull(m_dataError.Error);
            Assert.IsNull(m_dataError["Something"]);
            Assert.IsNull(m_dataError["MockProperty"]);
        }

        [Test]
        public void Test_SetError_with_no_error()
        {
            m_dataError.SetError(() => MockProperty, null);
            Assert.IsNull(m_dataError.Error);
            Assert.IsNull(m_dataError["MockProperty"]);
        }

        [Test]
        public void Test_SetError_with_error()
        {
            m_dataError.SetError(() => MockProperty, "My Error");
            Assert.AreEqual("My Error", m_dataError.Error);
            Assert.AreEqual("My Error", m_dataError["MockProperty"]);
            Assert.IsNull(m_dataError["Anything"]);
        }

        [Test]
        public void Test_SetError_can_change_error()
        {
            m_dataError.SetError(() => MockProperty, "My Error");
            m_dataError.SetError(() => MockProperty, "My New Error");
            Assert.AreEqual("My New Error", m_dataError.Error);
            Assert.AreEqual("My New Error", m_dataError["MockProperty"]);
            Assert.IsNull(m_dataError["Anything"]);
        }

        [Test]
        public void Test_SetError_can_reset_error()
        {
            m_dataError.SetError(() => MockProperty, "My Error");
            m_dataError.SetError(() => MockProperty, null);
            Assert.IsNull(m_dataError.Error);
            Assert.IsNull(m_dataError["MockProperty"]);
        }

        #endregion
    }
}
