using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Mox.Lobby.Network
{
    [TestFixture]
    public class ServerBackendTests
    {
        #region Variables

        private ServerBackend m_server;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_server = ServerBackend.CreateLocal();
        }

        #endregion

        #region Tests

#warning TODO: Test

        #endregion
    }
}
