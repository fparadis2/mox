using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;

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

        public void Update()
        {
            Console.WriteLine("Checking to see if we need to download {0}", FileName);

            if (!Download(Url, m_downloadFileTarget))
            {
                Console.WriteLine("{0} is up-to-date", FileName);
                return;
            }

            Console.WriteLine("Downloaded {0} successfully", FileName);

            File.Delete(DestinationFileName);
            ZipFile.ExtractToDirectory(m_downloadFileTarget, m_destinationFolder);

            Console.WriteLine("Unzipped {0} successfully", FileName);
        }

        private static bool Download(string url, string destinationFile)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";

            string etagFile = destinationFile + ".etag";
            if (File.Exists(etagFile))
            {
                string etag = File.ReadAllText(etagFile);
                request.Headers[HttpRequestHeader.IfNoneMatch] = etag;
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    using (var input = response.GetResponseStream())
                    using (var output = File.Create(destinationFile))
                    {
                        byte[] buffer = new byte[4096];

                        while (true)
                        {
                            int read = input.Read(buffer, 0, buffer.Length);

                            if (read <= 0)
                                break;

                            output.Write(buffer, 0, read);
                        }
                    }

                    string etag = response.Headers[HttpResponseHeader.ETag];
                    File.WriteAllText(etagFile, etag);
                }
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    using (HttpWebResponse response = (HttpWebResponse)e.Response)
                    {
                        if (response.StatusCode == HttpStatusCode.NotModified)
                        {
                            return false;
                        }
                    }
                }

                throw;
            }

            return true;
        }
    }
}
