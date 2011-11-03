using System;
using Mox.Replication;

namespace Mox
{
    /// <summary>
    /// Some useful extensions for tests
    /// </summary>
    public static class GameExtensions
    {
        public static Game Replicate(this Game game)
        {
            ReplicationSource<Player> source = new ReplicationSource<Player>(game, new OpenAccessControlStrategy<Player>());
            ReplicationClient<Game> client = new ReplicationClient<Game>();
            source.Register(null, client);
            return client.Host;
        }
    }
}
