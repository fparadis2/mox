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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media.Imaging;
using NUnit.Framework;

namespace Mox.UI
{
    [TestFixture]
    public class AsyncImageTests
    {
        #region Inner Types

        private class MockAsyncImage : AsyncImage
        {
            public readonly ManualResetEvent LoadHandle = new ManualResetEvent(false);

            public readonly ManualResetEvent FinishedLoadingHandle = new ManualResetEvent(false);

            public byte[] ImageData;
            public bool SuccessfulLoad;

            protected override bool Load(BitmapImage destination)
            {
                if (!SuccessfulLoad)
                {
                    return false;
                }

                Wait(LoadHandle);

                destination.StreamSource = new MemoryStream(ImageData);
                return true;
            }

            protected override void OnPropertyChanged(string propertyName)
            {
                base.OnPropertyChanged(propertyName);

                if (propertyName == "IsLoading")
                {
                    if (IsLoading)
                    {
                        FinishedLoadingHandle.Reset();
                    }
                    else
                    {
                        FinishedLoadingHandle.Set();
                    }
                }
            }
        }

        #endregion

        #region Variables

        private MemoryStream m_imageData;
        private MockAsyncImage m_asyncImage;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_imageData = new MemoryStream();
            Bitmap bitmap = new Bitmap(16, 16);
            bitmap.Save(m_imageData, ImageFormat.Jpeg);

            m_asyncImage = new MockAsyncImage 
            { 
                ImageData = m_imageData.ToArray()
            };
        }

        #endregion

        #region Utilities

        private static void Wait(WaitHandle handle)
        {
            Assert.IsTrue(handle.WaitOne(TimeSpan.FromSeconds(1)), "Test timeout!");
        }

        private static void Reload(MockAsyncImage mockAsyncImage)
        {
            mockAsyncImage.LoadHandle.Reset();
            mockAsyncImage.Reload();
        }

        private static void AllowLoad(MockAsyncImage mockAsyncImage)
        {
            mockAsyncImage.LoadHandle.Set();
            Wait(mockAsyncImage.FinishedLoadingHandle);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Image_is_loading_by_default()
        {
            Assert.IsTrue(m_asyncImage.IsLoading);
        }

        [Test]
        public void Test_ImageSource_is_null_while_being_loaded()
        {
            Assert.IsNull(m_asyncImage.ImageSource);
        }

        [Test]
        public void Test_Image_is_considered_loaded_even_if_it_cannot_be_loaded()
        {
            m_asyncImage.SuccessfulLoad = false;

            Assert.IsNull(m_asyncImage.ImageSource);
            Assert.IsNull(m_asyncImage.ImageSource);
            Assert.IsTrue(m_asyncImage.IsLoading);
            AllowLoad(m_asyncImage);

            Assert.IsFalse(m_asyncImage.IsLoading);
            Assert.IsNull(m_asyncImage.ImageSource);
        }

        [Test]
        public void Test_Accessing_the_image_source_triggers_a_loading()
        {
            m_asyncImage.SuccessfulLoad = true;

            Assert.IsNull(m_asyncImage.ImageSource);
            Assert.IsNull(m_asyncImage.ImageSource);
            Assert.IsTrue(m_asyncImage.IsLoading);
            AllowLoad(m_asyncImage);

            Assert.IsFalse(m_asyncImage.IsLoading);
            Assert.IsNotNull(m_asyncImage.ImageSource);
        }

        [Test]
        public void Test_Reload_triggers_a_reloading()
        {
            m_asyncImage.SuccessfulLoad = false;
            Assert.IsNull(m_asyncImage.ImageSource);
            AllowLoad(m_asyncImage);
            Assert.IsFalse(m_asyncImage.IsLoading);
            
            m_asyncImage.SuccessfulLoad = true;
            Reload(m_asyncImage);
            Assert.IsTrue(m_asyncImage.IsLoading);
            AllowLoad(m_asyncImage);
            Assert.IsFalse(m_asyncImage.IsLoading);
            Assert.IsNotNull(m_asyncImage.ImageSource);
        }

        [Test]
        public void Test_Reload_triggers_a_reloading_even_if_the_image_was_already_loaded()
        {
            m_asyncImage.SuccessfulLoad = true;
            Assert.IsNull(m_asyncImage.ImageSource);
            AllowLoad(m_asyncImage);
            Assert.IsFalse(m_asyncImage.IsLoading);
            Assert.IsNotNull(m_asyncImage.ImageSource);

            m_asyncImage.SuccessfulLoad = true;
            Reload(m_asyncImage);
            Assert.IsTrue(m_asyncImage.IsLoading);
            AllowLoad(m_asyncImage);
            Assert.IsFalse(m_asyncImage.IsLoading);
            Assert.IsNotNull(m_asyncImage.ImageSource);
        }

        [Test]
        public void Test_ImageSource_is_freezed_so_that_it_can_be_accessed_from_the_UI_thread()
        {
            m_asyncImage.SuccessfulLoad = true;

            Assert.IsNull(m_asyncImage.ImageSource);
            AllowLoad(m_asyncImage);
            Assert.IsNotNull(m_asyncImage.ImageSource);
            Assert.That(m_asyncImage.ImageSource.IsFrozen);
        }

        [Test]
        public void Test_ImageSource_property_changes_when_loaded()
        {
            m_asyncImage.SuccessfulLoad = true;

            Assert.IsNull(m_asyncImage.ImageSource);
            Assert.TriggersPropertyChanged(m_asyncImage, "ImageSource", () => AllowLoad(m_asyncImage));
        }

        [Test]
        public void Test_ImageSource_property_doesnt_change_when_loading_fails()
        {
            m_asyncImage.SuccessfulLoad = false;

            Assert.IsNull(m_asyncImage.ImageSource);
            Assert.DoesntTriggerPropertyChanged(m_asyncImage, "ImageSource", () => AllowLoad(m_asyncImage));
        }

        [Test]
        public void Test_ImageSource_property_changes_when_reloading()
        {
            m_asyncImage.SuccessfulLoad = true;

            Assert.IsNull(m_asyncImage.ImageSource);
            AllowLoad(m_asyncImage);

            Reload(m_asyncImage);
            Assert.TriggersPropertyChanged(m_asyncImage, "ImageSource", () => AllowLoad(m_asyncImage));
        }

        [Test]
        public void Test_IsLoading_property_changes_when_loaded()
        {
            m_asyncImage.SuccessfulLoad = true;

            Assert.IsNull(m_asyncImage.ImageSource);
            Assert.TriggersPropertyChanged(m_asyncImage, "IsLoading", () => AllowLoad(m_asyncImage));
        }

        [Test]
        public void Test_IsLoading_property_changes_when_loading_fails()
        {
            m_asyncImage.SuccessfulLoad = false;

            Assert.IsNull(m_asyncImage.ImageSource);
            Assert.TriggersPropertyChanged(m_asyncImage, "IsLoading", () => AllowLoad(m_asyncImage));
        }

        [Test]
        public void Test_IsLoading_property_changes_when_reloading()
        {
            m_asyncImage.SuccessfulLoad = true;

            Assert.IsNull(m_asyncImage.ImageSource);
            AllowLoad(m_asyncImage);

            Reload(m_asyncImage);
            Assert.TriggersPropertyChanged(m_asyncImage, "IsLoading", () => AllowLoad(m_asyncImage));
        }

        #endregion
    }
}
