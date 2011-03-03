using NUnit.Framework;

namespace Mox.Lobby
{
    [TestFixture]
    public class LocalClientTests : ClientTestsBase
    {
        protected override Server CreateServer(ILog log)
        {
            return Server.CreateLocal(log);
        }

        protected override Client CreateClient(Server server)
        {
            return Client.CreateLocal((LocalServer)server);
        }
    }
}