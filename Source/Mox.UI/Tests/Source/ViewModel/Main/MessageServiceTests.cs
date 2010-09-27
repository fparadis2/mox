using System;
using System.Windows;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.UI
{
    [TestFixture]
    public class MessageServiceTests
    {
        #region Variables

        private MockRepository m_mockery;
        private IMessageService m_mockInstance;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();
            m_mockInstance = m_mockery.StrictMock<IMessageService>();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.IsNotNull(MessageService.Instance);
        }

        [Test]
        public void Test_Can_Swap_instance()
        {
            var newInstance = m_mockery.StrictMock<IMessageService>();

            var oldInstance = MessageService.Instance;
            Assert.AreNotEqual(newInstance, oldInstance);

            using (MessageService.Use(newInstance))
            {
                Assert.AreEqual(newInstance, MessageService.Instance);
            }
            Assert.AreEqual(oldInstance, MessageService.Instance);
        }

        [Test]
        public void Test_ShowMessage_redirects_to_instance()
        {
            using (MessageService.Use(m_mockInstance))
            {
                Expect.Call(m_mockInstance.Show("Hello", "Caption", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation, MessageBoxResult.Cancel)).Return(MessageBoxResult.OK);
                m_mockery.Test(() => Assert.AreEqual(MessageBoxResult.OK, MessageService.ShowMessage("Hello", "Caption", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation, MessageBoxResult.Cancel)));

                Expect.Call(m_mockInstance.Show("Hello", "Caption", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation, MessageBoxResult.None)).Return(MessageBoxResult.OK);
                m_mockery.Test(() => Assert.AreEqual(MessageBoxResult.OK, MessageService.ShowMessage("Hello", "Caption", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation)));

                Expect.Call(m_mockInstance.Show("Hello", "Caption", MessageBoxButton.OKCancel, MessageBoxImage.None, MessageBoxResult.None)).Return(MessageBoxResult.OK);
                m_mockery.Test(() => Assert.AreEqual(MessageBoxResult.OK, MessageService.ShowMessage("Hello", "Caption", MessageBoxButton.OKCancel)));

                Expect.Call(m_mockInstance.Show("Hello", "Caption", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None)).Return(MessageBoxResult.OK);
                m_mockery.Test(() => Assert.AreEqual(MessageBoxResult.OK, MessageService.ShowMessage("Hello", "Caption")));

                Expect.Call(m_mockInstance.Show("Hello", null, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None)).Return(MessageBoxResult.OK);
                m_mockery.Test(() => Assert.AreEqual(MessageBoxResult.OK, MessageService.ShowMessage("Hello")));
            }
        }

        #endregion
    }
}
