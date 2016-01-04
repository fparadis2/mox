using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Media.Imaging;

namespace Mox.UI
{
    internal class ImageServiceImplementation
    {
        #region Variables

        private readonly ImageCache m_cache = new ImageCache();
        private readonly ImageStorage m_storage = new ImageStorage();
        private readonly ImageLoader m_loader;

        #endregion

        #region Constructor

        public ImageServiceImplementation()
        {
            m_loader = new ImageLoader(m_storage, m_cache);
        }

        #endregion

        #region Methods

        public BitmapSource LoadImage(ImageKey key, ImageLoadedCallback loadedCallback)
        {
            if (key.CachePolicy == ImageCachePolicy.Never)
            {
                return m_storage.LoadImage(key);
            }

            BitmapSource image;
            if (m_cache.TryGetValue(key, out image))
            {
                return image;
            }

            m_loader.QueueLoading(key, loadedCallback);
            return null;
        }

        public BitmapSource LoadImageSynchronous(ImageKey key)
        {
            if (key.CachePolicy == ImageCachePolicy.Never)
            {
                return m_storage.LoadImage(key);
            }

            BitmapSource image;
            if (m_cache.TryGetValue(key, out image))
            {
                return image;
            }

            image = m_storage.LoadImage(key);
            m_cache.Add(key, image);
            return image;
        }

        #endregion

        #region Inner Types

        private class ImageCache
        {
            #region Variables

            private readonly object m_lock = new object();
            private readonly PermanentCache m_permanentCache = new PermanentCache();
            private readonly PruningCache m_pruningCache = new PruningCache();

            #endregion

            #region Methods

            public bool TryGetValue(ImageKey key, out BitmapSource image)
            {
                lock (m_lock)
                {
                    return GetCache(key).TryGetValue(key, out image);
                }
            }

            public void Add(ImageKey key, BitmapSource image)
            {
                lock (m_lock)
                {
                    GetCache(key).Add(key, image);
                }
            }

            private ICache GetCache(ImageKey key)
            {
                switch (key.CachePolicy)
                {
                    case ImageCachePolicy.Always:
                        return m_permanentCache;

                    case ImageCachePolicy.Recent:
                        return m_pruningCache;

                    default:
                        throw new NotImplementedException();
                }
            }

            #endregion

            #region Inner Types

            private interface ICache
            {
                bool TryGetValue(ImageKey key, out BitmapSource image);
                void Add(ImageKey key, BitmapSource image);
            }

            private class PermanentCache : ICache
            {
                #region Variables

                private readonly Dictionary<ImageKey, BitmapSource> m_cache = new Dictionary<ImageKey, BitmapSource>();

                #endregion

                #region Implementation of ICache

                public bool TryGetValue(ImageKey key, out BitmapSource image)
                {
                    return m_cache.TryGetValue(key, out image);
                }

                public void Add(ImageKey key, BitmapSource image)
                {
                    m_cache[key] = image;
                }

                #endregion
            }

            private class PruningCache : ICache
            {
                #region Constants

                private const int CacheSizeLimit = 100 * 1024 * 1024; // 100 Mb

                #endregion

                #region Variables

                private readonly Dictionary<ImageKey, LinkedListNode<CachedBitmap>> m_cache = new Dictionary<ImageKey, LinkedListNode<CachedBitmap>>();
                private readonly LinkedList<CachedBitmap> m_recentlyUsed = new LinkedList<CachedBitmap>();

                private int m_currentMemorySize;

                #endregion

                #region Methods

                public bool TryGetValue(ImageKey key, out BitmapSource image)
                {
                    LinkedListNode<CachedBitmap> node;
                    bool result = m_cache.TryGetValue(key, out node);
                    image = node != null ? node.Value.Bitmap : null;
                    return result;
                }

                public void Add(ImageKey key, BitmapSource image)
                {
                    int newSize = EstimateImageMemorySize(image);

                    m_currentMemorySize += newSize;
                    PruneImages();

                    LinkedListNode<CachedBitmap> node;
                    if (m_cache.TryGetValue(key, out node))
                    {
                        m_recentlyUsed.Remove(node);
                    }

                    node = new LinkedListNode<CachedBitmap>(new CachedBitmap(key, image));
                    m_recentlyUsed.AddFirst(node);
                    m_cache[key] = node;
                }

                private void PruneImages()
                {
                    while (m_currentMemorySize > CacheSizeLimit)
                    {
                        var removed = RemoveLeastRecentlyUsedImage();
                        m_currentMemorySize -= EstimateImageMemorySize(removed);
                    }
                }

                private BitmapSource RemoveLeastRecentlyUsedImage()
                {
                    var lastNode = m_recentlyUsed.Last;
                    m_recentlyUsed.RemoveLast();
                    m_cache.Remove(lastNode.Value.Key);
                    return lastNode.Value.Bitmap;
                }

                private static int EstimateImageMemorySize(BitmapSource image)
                {
                    if (image == null)
                    {
                        return 0;
                    }

                    return image.PixelWidth * image.PixelHeight * 4; // 4 bytes per pixel
                }

                #endregion

                #region Inner Types

                private class CachedBitmap
                {
                    public readonly ImageKey Key;
                    public readonly BitmapSource Bitmap;

                    public CachedBitmap(ImageKey key, BitmapSource bitmap)
                    {
                        Key = key;
                        Bitmap = bitmap;
                    }
                }

                #endregion
            }

            #endregion
        }

        private class ImageLoader
        {
            #region Variables

            private readonly BlockingCollection<LoadingRequest> m_queue = new BlockingCollection<LoadingRequest>();
            private readonly ImageStorage m_storage;
            private readonly ImageCache m_cache;

            #endregion

            #region Constructor

            public ImageLoader(ImageStorage storage, ImageCache cache)
            {
                m_storage = storage;
                m_cache = cache;

                Thread loaderThread = new Thread(ThreadLoop) { IsBackground = true, Name = "Image Loader Thread" };
                loaderThread.Start();
            }

            #endregion

            #region Methods

            public void QueueLoading(ImageKey key, ImageLoadedCallback loadedCallback)
            {
                m_queue.Add(new LoadingRequest(key, loadedCallback));
            }

            private void ThreadLoop()
            {
                while (!m_queue.IsAddingCompleted)
                {
                    var request = m_queue.Take();

                    BitmapSource image;
                    if (!m_cache.TryGetValue(request.Key, out image))
                    {
                        image = m_storage.LoadImage(request.Key);
                        m_cache.Add(request.Key, image);
                    }
                    request.DoCallback(request.Key, image);
                }
            }

            #endregion

            #region Inner Types

            private class LoadingRequest
            {
                public readonly ImageKey Key;
                private readonly ImageLoadedCallback m_callback;

                public LoadingRequest(ImageKey key, ImageLoadedCallback callback)
                {
                    Key = key;
                    m_callback = callback;
                }

                public void DoCallback(ImageKey key, BitmapSource image)
                {
                    if (m_callback != null)
                    {
                        m_callback(key, image);
                    }
                }
            }
		 
            #endregion
        }

        #endregion
    }
}