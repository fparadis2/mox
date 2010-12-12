using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Mox.UI
{
    public static class DragSourceBehavior
    {
        #region Variables

        private static Point? m_startPoint;

        #endregion

        #region Properties

        public static readonly DependencyProperty DragSourceProperty = DependencyProperty.RegisterAttached("DragSource", typeof(IDragSource), typeof(DragSourceBehavior), new PropertyMetadata(null, OnDragSourceChanged));
        
        public static IDragSource GetDragSource(DependencyObject dependencyObject)
        {
            return (IDragSource)dependencyObject.GetValue(DragSourceProperty);
        }

        public static void SetDragSource(DependencyObject dependencyObject, IDragSource value)
        {
            dependencyObject.SetValue(DragSourceProperty, value);
        }

        public static readonly DependencyProperty CanDragProperty = DependencyProperty.RegisterAttached("CanDrag", typeof(bool), typeof(DragSourceBehavior), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

        public static bool GetCanDrag(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(CanDragProperty);
        }

        public static void SetCanDrag(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(CanDragProperty, value);
        }

        #endregion

        #region Event Handlers

        private static void OnDragSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var element = (UIElement)dependencyObject;

            if (e.NewValue != null)
            {
                element.PreviewMouseLeftButtonDown += PreviewMouseLeftButtonDown;
                element.PreviewMouseMove += PreviewMouseMove;
                element.MouseLeave += MouseLeave;
            }
            else
            {
                element.PreviewMouseLeftButtonDown -= PreviewMouseLeftButtonDown;
                element.PreviewMouseMove -= PreviewMouseMove;
                element.MouseLeave -= MouseLeave;
            }
        }

        private static void PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_startPoint = e.GetPosition(null);
        }

        private static void MouseLeave(object sender, MouseEventArgs e)
        {
            // Need to reset since the mouse left in order to prevent mouse movement 
            // in another element to pick drag an drop
            m_startPoint = null;
        }

        private static void PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || m_startPoint == null)
            {
                return;
            }

            if (!HasMouseMovedFarEnough(e))
            {
                return;
            }
            
            var dependencyObject = (FrameworkElement)sender;

            if (!GetCanDrag(dependencyObject))
            {
                return;
            }

            var dragSource = GetDragSource(dependencyObject);

            var effects = dragSource.DragEffects;
            if (effects == DragDropEffects.None)
            {
                return;
            }

            DragDrop.DoDragDrop(dependencyObject, dragSource.Data, effects);

        }

        private static bool HasMouseMovedFarEnough(MouseEventArgs e)
        {
            Debug.Assert(m_startPoint.HasValue);

            Vector delta = m_startPoint.Value - e.GetPosition(null);

            return Math.Abs(delta.X) > SystemParameters.MinimumHorizontalDragDistance ||
                   Math.Abs(delta.Y) > SystemParameters.MinimumVerticalDragDistance;
        }

        #endregion

    }

    public interface IDragSource
    {
        DragDropEffects DragEffects { get; }
        object Data { get; }
    }

    public class DragSource<T> : IDragSource
    {
        #region Variables

        private readonly Func<DragDropEffects> m_effectsProvider;
        private readonly Func<T> m_dataProvider;

        public DragSource(Func<T> dataProvider)
            : this(dataProvider, null)
        {
        }

        public DragSource(Func<T> dataProvider, Func<DragDropEffects> effectsProvider)
        {
            Throw.IfNull(dataProvider, "dataProvider");
            m_dataProvider = dataProvider;
            m_effectsProvider = effectsProvider;
        }

        #endregion

        #region Properties

        public DragDropEffects DragEffects
        {
            get
            {
                if (m_effectsProvider != null)
                {
                    return m_effectsProvider();
                }

                return DragDropEffects.All;
            }
        }

        object IDragSource.Data
        {
            get { return Data; }
        }

        public T Data
        {
            get { return m_dataProvider(); }
        }

        #endregion
    }
}
