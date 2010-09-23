using System;
using System.IO;

namespace Mox.Database
{
    public class DiskDeckStorageStrategy : IDeckStorageStrategy
    {
        private readonly string m_baseDirectory;

        public DiskDeckStorageStrategy(string directory)
        {
            m_baseDirectory = directory;
            Directory.CreateDirectory(directory);
        }

        public void LoadAll(Action<Stream, Guid> loadingAction)
        {
            foreach (string file in Directory.GetFiles(m_baseDirectory))
            {
                Guid guid = Guid.Parse(Path.GetFileName(file));

                using (Stream stream = File.OpenRead(file))
                {
                    loadingAction(stream, guid);
                }
            }
        }

        public Stream OpenWrite(Guid guid)
        {
            return File.Create(GetFilename(guid));
        }

        public void Delete(Guid guid)
        {
            File.Delete(GetFilename(guid));
        }

        private string GetFilename(Guid guid)
        {
            return Path.Combine(m_baseDirectory, guid.ToString());
        }
    }
}