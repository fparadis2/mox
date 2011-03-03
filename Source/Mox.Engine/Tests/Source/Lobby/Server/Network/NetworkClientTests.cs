using Mox.Lobby.Network;
using NUnit.Framework;

namespace Mox.Lobby
{
    //WCF doesn't play nice with tests (callbacks and threading)
    //[TestFixture]
    public class NetworkClientTests : ClientTestsBase
    {
        private const int TestPort = 13211;

        protected override Server CreateServer(ILog log)
        {
            NetworkServer server = Server.CreateNetwork(log);
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