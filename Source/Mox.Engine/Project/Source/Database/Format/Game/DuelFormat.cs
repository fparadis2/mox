namespace Mox
{
    internal class DuelFormat : IGameFormat
    {
        public string Name { get { return "Duel"; } }
        public int NumPlayers { get { return 2; } }
    }
}