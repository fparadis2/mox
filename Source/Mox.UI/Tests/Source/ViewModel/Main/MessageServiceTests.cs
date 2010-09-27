using System;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.UI
{
    [TestFixture]
    public class MessageServiceTests
    {
        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.IsNotNull(MessageService.Instance);
        }

        [Test]
        public void Test_Can_Swap_instance()
        {
            MockRepository mockery = new MockRepository();
            var newInstance = mockery.StrictMock<IMessageService>();

            var oldInstance = MessageService.Instance;
            Assert.AreNotEqual(newInstance, oldInstance);

            using (MessageService.Use(newInstance))
            {
                Assert.AreEqual(newInstance, MessageService.Instance);
            }
            Assert.AreEqual(oldInstance, MessageService.Instance);
        }

        #endregion
    }
}
