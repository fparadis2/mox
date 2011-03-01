using System;
using Rhino.Mocks;

namespace Mox.Lobby.Backend
{
    public class MockClient : IClient
    {
        #region Variables

        private readonly MockRepository m_mockery;
        private readonly User m_user;

        private readonly IChatClient m_chatClient;

        #endregion

        #region Constructor

        public MockClient(MockRepository mockery, User user)
        {
            m_mockery = mockery;
            m_user = user;

            m_chatClient = m_mockery.StrictMock<IChatClient>();
        }

        #endregion

        #region Properties

        public User User
        {
            get { return m_user; }
        }

        public IChatClient ChatClient
        {
            get { return m_chatClient; }
        }

        #endregion

        #region Expectations

        public void Expect_Chat_Message(User user, string msg)
        {
            ChatClient.OnMessageReceived(user, msg);
        }

        #endregion
    }
}
