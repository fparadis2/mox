using System;
using NUnit.Framework;

namespace Mox.Lobby.Network
{
    [TestFixture]
    public class ServerTests
    {
        #region Variables

        private Server m_server;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_server = Server.CreateLocal();
        }

        #endregion

        #region Tests

#warning TODO: Test

        #endregion
    }
}
