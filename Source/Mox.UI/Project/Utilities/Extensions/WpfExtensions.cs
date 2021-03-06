﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mox.UI
{
    public static class WpfExtensions
    {
        public static T FindDataContext<T>(this FrameworkElement child)
        {
            for (var parent = child; parent != null; parent = parent.Parent as FrameworkElement)
            {
                if (parent.DataContext is T)
                    return (T)parent.DataContext;
            }

            return default(T);
        }

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

        public static T FindVisualChild<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? child.FindVisualChild<T>();
                if (result != null)
                    return result;
            }
            return null;
        }

        public static bool IsChildOf(this FrameworkElement child, FrameworkElement parent)
        {
            DependencyObject p = child.Parent;

            while (p != null)
            {
                if (p == parent)
                    return true;

                var parentElement = p as FrameworkElement;
                if (parentElement == null)
                    break;

                p = parentElement.Parent;
            }

            return false;
        }

        public static object GetObjectDataFromPoint(this ItemsControl source, Point point)
        {
            DependencyObject element = source.InputHitTest(point) as DependencyObject;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    data = source.ItemContainerGenerator.ItemFromContainer(element);
                    if (data == DependencyProperty.UnsetValue)
                    {
                        element = VisualTreeHelper.GetParent(element);
                    }
                    if (element == source || element == null)
                    {
                        return null;
                    }
                }
                if (data != DependencyProperty.UnsetValue)
                {
                    return data;
                }
            }
            return null;
        }
    }
}
