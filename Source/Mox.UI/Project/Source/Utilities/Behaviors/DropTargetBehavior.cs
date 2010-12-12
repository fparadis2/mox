using System;
using System.Windows;

namespace Mox.UI
{
    public static class DropTargetBehavior
    {
        #region Properties

        public static readonly DependencyProperty DropTargetProperty = DependencyProperty.RegisterAttached("DropTarget", typeof(IDropTarget), typeof(DropTargetBehavior), new PropertyMetadata(null, OnDropTargetChanged));

        public static IDropTarget GetDropTarget(DependencyObject d)
        {
            return (IDropTarget)d.GetValue(DropTargetProperty);
        }

        public static void SetDropTarget(DependencyObject d, IDropTarget value)
        {
            d.SetValue(DropTargetProperty, value);
        }

        #endregion

        #region Event Handlers

        private static void OnDropTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (UIElement)d;

            if (e.NewValue != null)
            {
                element.PreviewDragOver += DragOver;
                element.PreviewDrop += Drop;
            }
            else
            {
                element.PreviewDragOver -= DragOver;
                element.PreviewDrop -= Drop;
            }
        }

        private static void Drop(object sender, DragEventArgs e)
        {
            var dropTarget = GetDropTarget((DependencyObject)sender);

            dropTarget.Drop(e.Data, e.KeyStates);
            e.Handled = true;
        }

        private static void DragOver(object sender, DragEventArgs e)
        {
            var dropTarget = GetDropTarget((DependencyObject)sender);

            e.Effects = dropTarget.GetDropEffects(e.Data, e.KeyStates);
            e.Handled = true;
        }

        #endregion

    }

    public interface IDropTarget
    {
        DragDropEffects GetDropEffects(IDataObject dataObject, Flags<DragDropKeyStates> modifiers);
        void Drop(IDataObject dataObject, Flags<DragDropKeyStates> modifiers);
    }

    public class DropTarget<T> : IDropTarget
    {
        #region Variables

        private readonly Func<T, Flags<DragDropKeyStates>, DragDropEffects> m_getDropEffectsFunctor;
        private readonly Action<T, Flags<DragDropKeyStates>> m_dropFunctor;

        public DropTarget(Action<T, Flags<DragDropKeyStates>> dropFunctor)
            : this(dropFunctor, null)
        {
        }

        public DropTarget(Action<T, Flags<DragDropKeyStates>> dropFunctor, Func<T, Flags<DragDropKeyStates>, DragDropEffects> getDropEffectsFunctor)
        {
            Throw.IfNull(dropFunctor, "dropFunctor");
            m_dropFunctor = dropFunctor;
            m_getDropEffectsFunctor = getDropEffectsFunctor;
        }

        #endregion

        #region Methods

        DragDropEffects IDropTarget.GetDropEffects(IDataObject dataObject, Flags<DragDropKeyStates> modifiers)
        {
            if (!dataObject.GetDataPresent(typeof(T)))
            {
                return DragDropEffects.None;
            }

            if (m_getDropEffectsFunctor != null)
            {
                return m_getDropEffectsFunctor((T)dataObject.GetData(typeof (T)), modifiers);
            }

            return DragDropEffects.All;
        }

        void IDropTarget.Drop(IDataObject dataObject, Flags<DragDropKeyStates> modifiers)
        {
            if (dataObject.GetDataPresent(typeof(T)))
            {
                m_dropFunctor((T)dataObject.GetData(typeof(T)), modifiers);
            }
        }

        #endregion
    }
}
