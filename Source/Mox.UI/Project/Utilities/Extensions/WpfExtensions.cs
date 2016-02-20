using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Mox.UI
{
    public static class WpfExtensions
    {
        public static T FindVisualParent<T>(this DependencyObject child) 
            where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            for (; parent != null; parent = VisualTreeHelper.GetParent(parent))
            {
                if (parent is T)
                    return (T) parent;
            }

            return null;
        }
    }
}
