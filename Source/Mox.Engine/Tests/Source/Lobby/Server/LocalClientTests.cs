using NUnit.Framework;

namespace Mox.Lobby
{
    [TestFixture]
    public class LocalClientTests : ClientTestsBase
    {
        protected override Server CreateServer()
        {
            return Server.CreateLocal();
        }

        protected override Client CreateClient(Server server)
        {
            return Client.CreateLocal(server);
        }
    }
}