using Mox.Database;
using Mox.Database.Internal;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;

namespace Mox
{
    public class CardDatabaseFileUpdater
    {
        private const string Url = "http://mtgjson.com/json/AllSets.json.zip";
        private const string FileName = "AllSets.json.zip";

        private readonly string m_destinationFolder;
        private readonly string m_downloadFileTarget;

        public CardDatabaseFileUpdater()
        {
            string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            m_downloadFileTarget = Path.Combine(exePath, "AllSets.json.zip");
            m_destinationFolder = Path.Combine(exePath, "../../../Source/Mox.Engine/Project/Source/Database/Data");
        }

        public string DestinationFileName
        {
            get { return Path.Combine(m_destinationFolder, "AllSets.json"); }
        }

        public static CardDatabase UpdateDatabase()
        {
            var jsonFile = UpdateCardDatabaseFile();
            return ParseCardDatabase(jsonFile);
        }

        private static string UpdateCardDatabaseFile()
        {
            CardDatabaseFileUpdater updater = new CardDatabaseFileUpdater();
            updater.Update();
            return updater.DestinationFileName;
        }

        private static CardDatabase ParseCardDatabase(string filename)
        {
            CardDatabase database = new CardDatabase();

            using (var stream = File.OpenRead(filename))
            {
                JsonParser parser = new JsonParser(database);
                parser.Parse(stream);
            }

            return database;
        }

        public void Update()
        {
            Console.WriteLine("Checking to see if we need to download {0}", FileName);

            if (!DownloadHelper.Download(Url, m_downloadFileTarget))
            {
                Console.WriteLine("{0} is up-to-date", FileName);
                return;
            }

            Console.WriteLine("Downloaded {0} successfully", FileName);

            File.Delete(DestinationFileName);
            ZipFile.ExtractToDirectory(m_downloadFileTarget, m_destinationFolder);

            Console.WriteLine("Unzipped {0} successfully", FileName);
        }
    }
}
