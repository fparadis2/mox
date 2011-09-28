using System;
using Mox.Lobby;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.UI.Lobby
{
    [TestFixture]
    public class LobbyChatViewModelTests
    {
        #region Variables

        private MockRepository m_mockery;

        private IChatService m_chat;
        private LobbyChatViewModel m_viewModel;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();
            m_chat = m_mockery.StrictMock<IChatService>();

            m_viewModel = new LobbyChatViewModel { ChatService = m_chat };
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.IsNull(m_viewModel.Text);
            Assert.IsNull(m_viewModel.Input);
        }

        [Test]
        public void Test_INotifyPropertyChanged_implementation()
        {
            Assert.ThatAllPropertiesOn(m_viewModel).RaiseChangeNotification();
        }

        [Test]
        public void Test_Say_does_nothing_if_no_input()
        {
            Assert.IsNull(m_viewModel.Input, "Sanity check");

            using (m_mockery.Test())
            {
                Assert.IsFalse(m_viewModel.CanSay());
                m_viewModel.Say();
            }
        }

        [Test]
        public void Test_Say_sends_the_input_to_the_chat_service()
        {
            m_viewModel.Input = "Hello World!";

            m_chat.Say("Hello World!");

            using (m_mockery.Test())
            {
                Assert.IsTrue(m_viewModel.CanSay());
                m_viewModel.Say();
            }

            Assert.IsNullOrEmpty(m_viewModel.Input);
        }

        #endregion
    }
}
