using System;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.Tests
{
    [TestFixture]
    public class LogExtensionsTests
    {
        #region Variables

        private MockRepository m_mockery;
        private ILog m_log;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();
            m_log = m_mockery.StrictMock<ILog>();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Log_logs_a_formatted_message()
        {
            m_log.Log(new LogMessage { Text = "Hello world", Importance = LogImportance.High });

            using (m_mockery.Test())
            {
                m_log.Log(LogImportance.High, "Hello {0}", "world");
            }
        }

        [Test]
        public void Test_LogError_logs_an_error()
        {
            m_log.Log(new LogMessage { Text = "Hello world", Importance = LogImportance.Error });

            using (m_mockery.Test())
            {
                m_log.LogError("Hello {0}", "world");
            }
        }

        [Test]
        public void Test_LogWarning_logs_an_error()
        {
            m_log.Log(new LogMessage { Text = "Hello world", Importance = LogImportance.Warning });

            using (m_mockery.Test())
            {
                m_log.LogWarning("Hello {0}", "world");
            }
        }

        #endregion
    }
}
