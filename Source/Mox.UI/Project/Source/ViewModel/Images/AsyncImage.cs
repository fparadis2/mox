// Copyright (c) François Paradis
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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Mox.UI
{
    /// <summary>
    /// Provides an easy way to asynchronously load images.
    /// </summary>
    public abstract class AsyncImage : ViewModel, IAsyncImage
    {
        #region Inner Types

        private enum ImageState
        {
            Initialized,
            ScheduledForLoading,
            Loaded
        }

        private class FileAsyncImage : AsyncImage
        {
            private readonly string m_filename;

            public FileAsyncImage(string filename)
            {
                Throw.IfEmpty(filename, "filename");
                m_filename = filename;
            }

            protected override bool Load(BitmapImage destination)
            {
                if (File.Exists(m_filename))
                {
                    destination.UriSource = new Uri(m_filename);
                    return true;
                }

                return false;
            }
        }

        private class EmptyAsyncImage : AsyncImage
        {
            protected override bool Load(BitmapImage destination)
            {
                return false;
            }
        }

        #endregion

        #region Variables

        private int m_state = (int)ImageState.Initialized;
        private ImageSource m_imageSource;

        private static readonly AsyncOperation m_operation = AsyncOperationManager.CreateOperation(null);
        private static readonly EmptyAsyncImage m_empty = new EmptyAsyncImage();

        #endregion

        #region Properties

        /// <summary>
        /// Returns true while the image is loading.
        /// </summary>
        public bool IsLoading
        {
            get { return m_state != (int)ImageState.Loaded; }
        }

        /// <summary>
        /// The image, when it has been loaded.
        /// </summary>
        public ImageSource ImageSource
        {
            get 
            {
                ScheduleLoadingIf(ImageState.Initialized);
                return m_imageSource; 
            }
            private set 
            {
                if (m_imageSource != value)
                {
                    m_imageSource = value;
                    OnPropertyChanged("ImageSource");
                }
            }
        }

        #endregion

        #region Methods

        private bool SetState(ImageState expectedState, ImageState newState)
        {
            bool result = Interlocked.CompareExchange(ref m_state, (int)newState, (int)expectedState) == (int)expectedState;
            if (result)
            {
                OnPropertyChanged("IsLoading");
            }
            return result;
        }

        /// <summary>
        /// Forces the image to reload.
        /// </summary>
        public void Reload()
        {
            ScheduleLoadingIf(ImageState.Loaded);
        }

        private void ScheduleLoadingIf(ImageState currentState)
        {
            if (SetState(currentState, ImageState.ScheduledForLoading))
            {
                ThreadPool.QueueUserWorkItem(s => Load());
            }
        }

        private void Load()
        {
            Debug.Assert(m_state == (int)ImageState.ScheduledForLoading);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // Forces the load on EndInit

            if (Load(bitmap))
            {
                bitmap.EndInit();
                bitmap.Freeze();
            }
            else
            {
                bitmap = null;
            }

            m_operation.Post(FinishLoading, bitmap);
        }

        private void FinishLoading(object state)
        {
            ImageSource = (BitmapImage)state;
            Trace.Assert(SetState(ImageState.ScheduledForLoading, ImageState.Loaded));
        } 

        /// <summary>
        /// Loads the image into <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination"></param>
        /// <returns>True if image was loaded, false otherwise.</returns>
        protected abstract bool Load(BitmapImage destination);

        #endregion

        #region Static Creation

        /// <summary>
        /// Returns an image loaded from a file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static AsyncImage FromFile(string filename)
        {
            return new FileAsyncImage(filename);
        }

        /// <summary>
        /// An empty image.
        /// </summary>
        public static AsyncImage Empty
        {
            get { return m_empty; }
        }

        #endregion
    }
}
