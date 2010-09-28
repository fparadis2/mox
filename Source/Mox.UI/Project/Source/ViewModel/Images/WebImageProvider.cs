﻿// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;

namespace Mox.UI
{
    internal abstract class WebImageProvider
    {
        #region Variables

        private readonly Dictionary<string, WeakReference> m_imagesBeingDownloaded = new Dictionary<string, WeakReference>();

        #endregion

        #region Methods

        protected IAsyncImage GetImage(string cacheFileName, string url)
        {
            if (File.Exists(cacheFileName))
            {
                return AsyncImage.FromFile(cacheFileName);
            }

            lock (m_imagesBeingDownloaded)
            {
                WeakReference imageReference;
                AsyncImage image;

                if (m_imagesBeingDownloaded.TryGetValue(cacheFileName, out imageReference))
                {
                    image = (AsyncImage)imageReference.Target;

                    if (image != null)
                    {
                        return image;
                    }
                }

                image = new CachedAsyncImage(url, cacheFileName);
                m_imagesBeingDownloaded[cacheFileName] = new WeakReference(image);

                return image;
            }
        }

        #endregion

        #region Inner Types

        private class CachedAsyncImage : AsyncImage
        {
            private readonly string m_imageUrl;
            private readonly string m_cacheFilename;

            public CachedAsyncImage(string imageUrl, string cacheFilename)
            {
                m_imageUrl = imageUrl;
                m_cacheFilename = cacheFilename;
            }

            protected override bool Load(BitmapImage destination)
            {
                try
                {
                    DownloadImage();
                    destination.UriSource = new Uri(m_cacheFilename);
                    return true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Could not download image {0}: {1}", m_imageUrl, e.Message);
                    return false;
                }
            }

            private void DownloadImage()
            {
                Directory.CreateDirectory(Path.GetDirectoryName(m_cacheFilename));

                string tempFile = m_cacheFilename + ".downloading";

                WebClient client = new WebClient();
                using (Stream stream = client.OpenRead(m_imageUrl))
                using (Stream fileStream = File.Create(tempFile))
                {
                    stream.CopyTo(fileStream);
                }

                File.Move(tempFile, m_cacheFilename);
            }
        }

        #endregion
    }
}