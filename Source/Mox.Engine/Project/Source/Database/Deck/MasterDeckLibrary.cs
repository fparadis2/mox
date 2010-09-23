namespace Mox.Database
{
    public class MasterDeckLibrary : DeckLibrary
    {
        private static readonly DeckLibrary m_instance = new MasterDeckLibrary();

        private MasterDeckLibrary()
            : base(new DiskDeckStorageStrategy("Decks"))
        {
            Load();
        }

        public static DeckLibrary Instance
        {
            get { return m_instance; }
        }
    }
}