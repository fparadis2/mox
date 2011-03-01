using System;
using NUnit.Framework;

namespace Mox.Lobby.Network
{
    [TestFixture]
    public class LocalServerAdapterTests
    {
        #region Variables

        private LocalServerAdapter m_adapter;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            LocalOperationContext.Current = null;
            m_adapter = new LocalServerAdapter();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Adapter_takes_its_values_from_the_current_context()
        {
            object callback1 = new object();
            LocalOperationContext.Current = new LocalOperationContext(callback1);

            Assert.AreEqual(LocalOperationContext.Current.SessionId, m_adapter.SessionId);
            Assert.AreEqual(callback1, m_adapter.GetCallback<object>());

            object callback2 = new object();
            LocalOperationContext.Current = new LocalOperationContext(callback2);

            Assert.AreEqual(LocalOperationContext.Current.SessionId, m_adapter.SessionId);
            Assert.AreEqual(callback2, m_adapter.GetCallback<object>());
        }

        [Test]
        public void Test_Adapter_throws_if_no_current_local_operation_context()
        {
            Assert.Throws<InvalidOperationException>(() => m_adapter.SessionId.GetHashCode());
            Assert.Throws<InvalidOperationException>(() => m_adapter.GetCallback<object>());
        }

        #endregion
    }
}
