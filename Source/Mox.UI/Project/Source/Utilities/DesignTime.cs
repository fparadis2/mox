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

namespace Mox.UI
{
    /// <summary>  
    /// Allows the use of design-time only data binding... seems to only work  
    /// in Blend and not the VS designer...  
    /// </summary>  
    public class DesignTime
    {
        #region Variables

        public static readonly DependencyProperty DataContextProperty = DependencyProperty.RegisterAttached("DataContext", typeof(object), typeof(DesignTime), new PropertyMetadata(OnDataContextChanged));

        #endregion

        #region Methods

        private static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SetDataContext(d, e.NewValue);
        }

        public static void SetDataContext(DependencyObject d, object value)
        {
            var element = d as FrameworkElement;
            if (element == null) return;

            if (DesignerProperties.GetIsInDesignMode(element) || Application.Current == null || Application.Current.GetType() == typeof(Application))
            {
                element.DataContext = value;
            }
        }

        public static object GetDataContext(DependencyObject d)
        {
            var element = d as FrameworkElement;
            if (element == null) return null;

            return element.DataContext;
        }

        #endregion
    }
}
