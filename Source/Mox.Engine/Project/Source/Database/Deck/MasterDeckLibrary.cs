using System;
using System.IO;

namespace Mox.Database
{
    public class MasterDeckLibrary : DeckLibrary
    {
        private static readonly DeckLibrary m_instance = new MasterDeckLibrary();

        private static string DeckDirectory
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mox", "Decks");
            }
        }

        private MasterDeckLibrary()
            : base(new DiskDeckStorageStrategy(DeckDirectory))
        {
            Load();
        }

        public static DeckLibrary Instance
        {
            get { return m_instance; }
        }
    }
}