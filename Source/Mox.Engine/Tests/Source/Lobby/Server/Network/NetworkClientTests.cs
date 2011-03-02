using Mox.Lobby.Network;
using NUnit.Framework;

namespace Mox.Lobby
{
    [TestFixture]
    public class NetworkClientTests : ClientTestsBase
    {
        private const int TestPort = 13211;

        protected override Server CreateServer()
        {
            NetworkServer server = Server.CreateNetwork();
            server.Port = TestPort;
            return server;
        }

        protected override Client CreateClient(Server server)
        {
            NetworkClient client = Client.CreateNetwork();
            client.Port = TestPort;
            return client;
        }
    }
}