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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Mox.UI
{
    public class MainMenuListBox : ListBox
    {
        #region Constructor

        static MainMenuListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainMenuListBox), new FrameworkPropertyMetadata(typeof(MainMenuListBox)));
        }

        public MainMenuListBox()
        {
            ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty SelectedItemPositionProperty = DependencyProperty.Register("SelectedItemPosition", typeof(double), typeof(MainMenuListBox));

        public double SelectedItemPosition
        {
            get { return (double)GetValue(SelectedItemPositionProperty); }
        }

        #endregion

        #region Methods

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            UpdateSelectedItemPosition();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            // Disables all keyboard input for listboxes
            e.Handled = true;
            base.OnPreviewKeyDown(e);
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            UpdateSelectedItemPosition();

            base.OnSelectionChanged(e);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            Size result = base.ArrangeOverride(arrangeBounds);
            UpdateSelectedItemPosition();
            return result;
        }

        private void UpdateSelectedItemPosition()
        {
            if (ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                ListBoxItem lbi = (ListBoxItem)ItemContainerGenerator.ContainerFromItem(SelectedItem);

                if (lbi != null)
                {
                    Point pt = lbi.TranslatePoint(new Point(0, lbi.ActualHeight / 2), this);
                    SetValue(SelectedItemPositionProperty, pt.Y);
                }
            }
        }

        void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            UpdateSelectedItemPosition();
        }

        #endregion
    }
}
