using System;
using System.IO;
using System.Net;

namespace Mox
{
    public static class DownloadHelper
    {
        public static bool Download(string url, string destinationFile)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";

            string etagFile = destinationFile + ".etag";
            if (File.Exists(etagFile))
            {
                string etag = File.ReadAllText(etagFile);
                request.Headers[HttpRequestHeader.IfNoneMatch] = etag;
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
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
