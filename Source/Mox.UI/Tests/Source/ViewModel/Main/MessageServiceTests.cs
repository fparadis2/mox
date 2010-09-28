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
