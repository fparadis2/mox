using System;
using NUnit.Framework;

namespace Mox.Lobby.Network
{
    [TestFixture]
    public class LocalOperationContextTests
    {
        #region Variables

        private LocalOperationContext m_context;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_context = new LocalOperationContext(this);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_get_and_set_the_static_instance()
        {
            LocalOperationContext.Current = m_context;
            Assert.AreSame(m_context, LocalOperationContext.Current);
        }

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreSame(this, m_context.GetCallback<LocalOperationContextTests>());
        }

        [Test]
        public void Test_SessionId_is_always_different()
        {
            Assert.AreNotEqual(m_context.SessionId, new LocalOperationContext(new object()).SessionId);
        }

        #endregion
    }
}
