namespace Mox
{
    internal class DuelFormat : IGameFormat
    {
        public string Name { get { return "Duel"; } }
        public string Description { get { return "Classic two-player duel in a single match"; } }
        public int NumPlayers { get { return 2; } }
    }
}