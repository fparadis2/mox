using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Mox.UI
{
    public static class AvatarGenerator
    {
        private const int ImageSize = 100;

        private static string CachePath
        {
            get { return Path.Combine(ImageService.CachePath, "avatars"); }
        }

        static AvatarGenerator()
        {
            Directory.CreateDirectory(CachePath);
        }

        public static async Task<byte[]> GetAvatar(string identifier)
        {
            identifier = MangleIdentifier(identifier);

            string filename = identifier + ".png";
            string cacheFilename = Path.Combine(CachePath, filename);

            var result = await TryLoadFile(cacheFilename);

            if (result == null)
            {
                result = await TryGenerateAvatar(filename);

                if (result != null)
                {
                    await WriteToFile(cacheFilename, result);
                }
            }

            return result;
        }

        private static string MangleIdentifier(string identifier)
        {
            return identifier.GetHashCode().ToString(CultureInfo.InvariantCulture);
        }

        private static async Task<byte[]> TryLoadFile(string filename)
        {
            if (File.Exists(filename))
            {
                try
                {
                    using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                    {
                        byte[] buff = new byte[file.Length];
                        int read = await file.ReadAsync(buff, 0, (int)file.Length);
                        Debug.Assert(read == buff.Length);
                        return buff;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        private static async Task WriteToFile(string filename, byte[] data)
        {
            try
            {
                using (var file = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Write, 4096, true))
                {
                    await file.WriteAsync(data, 0, data.Length);
                }
            }
            catch
            {
            }
        }

        private static Task<byte[]> TryGenerateAvatar(string filename)
        {
            string url = string.Format("https://api.adorable.io/avatars/{0}/{1}", ImageSize, filename);

            WebClient client = new WebClient();
            return client.DownloadDataTaskAsync(url);
        }
    }
}
