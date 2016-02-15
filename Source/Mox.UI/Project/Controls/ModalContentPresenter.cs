using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace Mox.UI
{
    [ContentProperty("Content")]
    public class ModalContentPresenter : FrameworkElement
    {
        #region Variables

        private readonly Panel m_root = new ModalContentPresenterPanel();
        private readonly ContentPresenter m_primaryContentPresenter = new ContentPresenter();

        private readonly List<ModalContent> m_modalContents = new List<ModalContent>();

        private static readonly TraversalRequest ms_traversalRequest = new TraversalRequest(FocusNavigationDirection.First);

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(ModalContentPresenter), new PropertyMetadata(null, OnContentChanged));

        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModalContentPresenter presenter = (ModalContentPresenter)d;
            presenter.OnContentChanged(e);
        }

        private void OnContentChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                RemoveLogicalChild(e.OldValue);
            }

            m_primaryContentPresenter.Content = e.NewValue;

            if (e.NewValue != null)
            {
                AddLogicalChild(e.NewValue);
            }
        }

        public static readonly DependencyProperty ModalOverlayBrushProperty = DependencyProperty.Register("ModalOverlayBrush", typeof(Brush), typeof(ModalContentPresenter), new PropertyMetadata(new SolidColorBrush(System.Windows.Media.Color.FromArgb(204, 169, 169, 169))));

        public Brush ModalOverlayBrush
        {
            get { return (Brush) GetValue(ModalOverlayBrushProperty); }
            set { SetValue(ModalOverlayBrushProperty, value); }
        }

        #endregion

        #region Constructor

        public ModalContentPresenter()
        {
            AddVisualChild(m_root);
            m_root.Children.Add(m_primaryContentPresenter);
        }

        #endregion

        #region Methods

        protected override Visual GetVisualChild(int index)
        {
            return m_root;
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override IEnumerator LogicalChildren
        {
            get
            {
                var content = Content;
                if (content != null)
                    yield return content;

                foreach (var modalContent in m_modalContents)
                {
                    yield return modalContent;
                }
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            m_root.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            m_root.Measure(availableSize);
            return m_root.DesiredSize;
        }

        public void PushModalContent(object content)
        {
            var cachedFocus = DisableTopmostFocus();

            AddLogicalChild(content);

            ContentPresenter modalContentPresenter = new ContentPresenter { Content = content };
            Border border = new Border { Background = ModalOverlayBrush, Child = modalContentPresenter };
            m_root.Children.Add(border);
            border.MoveFocus(ms_traversalRequest);

            m_modalContents.Add(new ModalContent { Content = content, Visual = border, CachedFocus = cachedFocus });
        }

        public void PopModalContent(object content)
        {
            Debug.Assert(m_modalContents.Count > 0);

            var modalContent = m_modalContents[m_modalContents.Count - 1];
            Debug.Assert(modalContent.Content == content);

            m_modalContents.RemoveAt(m_modalContents.Count - 1);
            m_root.Children.Remove(modalContent.Visual);
            RemoveLogicalChild(modalContent.Content);

            RestoreTopmostFocus(modalContent.CachedFocus);
        }

        private CachedFocus DisableTopmostFocus()
        {
            UIElement topmostElement = m_root.Children[m_root.Children.Count - 1];

            CachedFocus focus = new CachedFocus
            {
                TabNavigation = KeyboardNavigation.GetTabNavigation(topmostElement),
                DirectionalNavigation = KeyboardNavigation.GetDirectionalNavigation(topmostElement),
                FocusedElement = Keyboard.FocusedElement
            };

            KeyboardNavigation.SetTabNavigation(topmostElement, KeyboardNavigationMode.None);
            KeyboardNavigation.SetDirectionalNavigation(topmostElement, KeyboardNavigationMode.None);

            Keyboard.ClearFocus();

            return focus;
        }

        private void RestoreTopmostFocus(CachedFocus cachedFocus)
        {
            UIElement topmostElement = m_root.Children[m_root.Children.Count - 1];

            KeyboardNavigation.SetTabNavigation(topmostElement, cachedFocus.TabNavigation);
            KeyboardNavigation.SetDirectionalNavigation(topmostElement, cachedFocus.DirectionalNavigation);

            Keyboard.Focus(cachedFocus.FocusedElement);

            topmostElement.MoveFocus(ms_traversalRequest);
        }

        #endregion

        #region Nested Types

        private struct ModalContent
        {
            public object Content;
            public UIElement Visual;
            public CachedFocus CachedFocus;
        }

        private struct CachedFocus
        {
            public KeyboardNavigationMode TabNavigation;
            public KeyboardNavigationMode DirectionalNavigation;
            public IInputElement FocusedElement;
        }

        private class ModalContentPresenterPanel : Panel
        {
            protected override Size MeasureOverride(Size availableSize)
            {
                Size resultSize = new Size();

                foreach (UIElement child in Children)
                {
                    child.Measure(availableSize);
                    resultSize.Width = Math.Max(resultSize.Width, child.DesiredSize.Width);
                    resultSize.Height = Math.Max(resultSize.Height, child.DesiredSize.Height);
                }

                return resultSize;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                foreach (UIElement child in InternalChildren)
                {
                    child.Arrange(new Rect(finalSize));
                }

                return finalSize;
            }
        }

        #endregion
    }
}
