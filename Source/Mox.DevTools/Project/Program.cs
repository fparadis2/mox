using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mox.Database;
using Mox.Database.Internal;

namespace Mox
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            program.Run();
        }

        private void Run()
        {
            var jsonFile = UpdateCardDatabaseFile();
            CardDatabase database = ParseCardDatabase(jsonFile);
        }

        private string UpdateCardDatabaseFile()
        {
            CardDatabaseFileUpdater updater = new CardDatabaseFileUpdater();
            updater.Update();
            return updater.DestinationFileName;
        }

        private CardDatabase ParseCardDatabase(string filename)
        {
            CardDatabase database = new CardDatabase();

            using (var stream = File.OpenRead(filename))
            {
                JsonParser parser = new JsonParser(database);
                parser.Parse(stream);
            }

            return database;
        }
    }
}
