using Mox.Lobby.Server;
using NUnit.Framework;

namespace Mox.Lobby.Client
{
    [TestFixture]
    public class LocalClientTests : ClientTestsBase
    {
        protected override Server.Server CreateServer(ILog log)
        {
            return Server.Server.CreateLocal(log);
        }

        protected override Client CreateClient(Server.Server server)
        {
            return Client.CreateLocal((LocalServer)server);
        }
    }
}