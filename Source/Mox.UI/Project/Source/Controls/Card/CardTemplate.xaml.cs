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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace Mox.UI
{
	public partial class CardTemplate
    {
        #region Constructor

        public CardTemplate()
		{
			this.InitializeComponent();

			// Insert code required on object creation below this point.
        }

        #endregion

        #region Properties

        public CardControl CardControl
        {
            get { return m_cardControl; }
        }

        #endregion

        #region Zoom Adorner

        private CardZoomAdorner m_zoomAdorner;

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.ChangedButton == MouseButton.Right)
            {
                SetZoomAdorner(true);
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                SetZoomAdorner(false);
            }

            base.OnMouseUp(e);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);

            SetZoomAdorner(false);
        }

        private void SetZoomAdorner(bool set)
        {
            if (set)
            {
                if (!CaptureMouse())
                {
                    return;
                }

                if (m_zoomAdorner == null)
                {
                    m_zoomAdorner = new CardZoomAdorner(this);
                    AdornerLayer parentAdorner = AdornerLayer.GetAdornerLayer(this);
                    parentAdorner.Add(m_zoomAdorner);
                    Opacity = 0;
                }

                Debug.Assert(m_zoomAdorner != null);
                m_zoomAdorner.FadeIn();
            }
            else if (m_zoomAdorner != null)
            {
                ReleaseMouseCapture();

                m_zoomAdorner.FadeOut(() =>
                {
                    if (m_zoomAdorner != null)
                    {
                        AdornerLayer parentAdorner = AdornerLayer.GetAdornerLayer(this);
                        parentAdorner.Remove(m_zoomAdorner);
                        m_zoomAdorner = null;
                        Opacity = 1;
                    }
                });
            }
        }

        #endregion
    }
}