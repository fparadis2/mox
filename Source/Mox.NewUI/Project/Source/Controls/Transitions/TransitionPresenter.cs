using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace Mox.UI
{
    [ContentProperty("Content")]
    public class TransitionPresenter : FrameworkElement
    {
        #region Variables

        private readonly UIElementCollection m_children;

        private AdornerDecorator m_currentHost;
        private AdornerDecorator m_previousHost;

        private Transition _activeTransition;

        #endregion

        #region Dependency Properties

        #region Content

        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(TransitionPresenter), new UIPropertyMetadata(null, OnContentChanged, CoerceContent));

        // Don't update content until done transitioning
        private static object CoerceContent(object element, object value)
        {
            TransitionPresenter te = (TransitionPresenter)element;
            if (te.IsTransitioning)
            {
                return te.CurrentContentPresenter.Content;
            }
            return value;
        }

        private static void OnContentChanged(object element, DependencyPropertyChangedEventArgs e)
        {
            TransitionPresenter te = (TransitionPresenter)element;
            te.BeginTransition();
        } 

        #endregion

        #region ContentTemplate

        public DataTemplate ContentTemplate
        {
            get { return (DataTemplate)GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }

        public static readonly DependencyProperty ContentTemplateProperty = DependencyProperty.Register("ContentTemplate", typeof(DataTemplate), typeof(TransitionPresenter), new UIPropertyMetadata(null, OnContentTemplateChanged));

        private static void OnContentTemplateChanged(object element, DependencyPropertyChangedEventArgs e)
        {
            TransitionPresenter te = (TransitionPresenter)element;
            te.CurrentContentPresenter.ContentTemplate = (DataTemplate)e.NewValue;
        } 

        #endregion

        #region ContentTemplateSelector

        public DataTemplateSelector ContentTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ContentTemplateSelectorProperty); }
            set { SetValue(ContentTemplateSelectorProperty, value); }
        }

        public static readonly DependencyProperty ContentTemplateSelectorProperty =
            DependencyProperty.Register("ContentTemplateSelector",
                typeof(DataTemplateSelector),
                typeof(TransitionPresenter),
                new UIPropertyMetadata(null, OnContentTemplateSelectorChanged));

        private static void OnContentTemplateSelectorChanged(object element, DependencyPropertyChangedEventArgs e)
        {
            TransitionPresenter te = (TransitionPresenter)element;
            te.CurrentContentPresenter.ContentTemplateSelector = (DataTemplateSelector)e.NewValue;
        }

        #endregion

        #region IsTransitioning

        public bool IsTransitioning
        {
            get { return (bool)GetValue(IsTransitioningProperty); }
            private set { SetValue(IsTransitioningPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey IsTransitioningPropertyKey =
            DependencyProperty.RegisterReadOnly("IsTransitioning",
                typeof(bool),
                typeof(TransitionPresenter),
                new UIPropertyMetadata(false));

        public static readonly DependencyProperty IsTransitioningProperty = IsTransitioningPropertyKey.DependencyProperty;

        #endregion

        #region Transition

        public Transition Transition
        {
            get { return (Transition)GetValue(TransitionProperty); }
            set { SetValue(TransitionProperty, value); }
        }

        public static readonly DependencyProperty TransitionProperty = DependencyProperty.Register("Transition", typeof(Transition), typeof(TransitionPresenter), new UIPropertyMetadata(null, null, CoerceTransition));

        private static object CoerceTransition(object element, object value)
        {
            TransitionPresenter te = (TransitionPresenter)element;
            if (te.IsTransitioning) return te._activeTransition;
            return value;
        }

        #endregion

        #endregion

        #region Constructor

        static TransitionPresenter()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(TransitionPresenter), new FrameworkPropertyMetadata(null, CoerceClipToBounds));
        }

        public TransitionPresenter()
        {
            m_children = new UIElementCollection(this, null);
            ContentPresenter currentContent = new ContentPresenter();
            m_currentHost = new AdornerDecorator { Child = currentContent };
            m_children.Add(m_currentHost);

            ContentPresenter previousContent = new ContentPresenter();
            m_previousHost = new AdornerDecorator { Child = previousContent };
        }

        #endregion

        #region Methods

        // Force clip to be true if the active Transition requires it
        private static object CoerceClipToBounds(object element, object value)
        {
            TransitionPresenter transitionElement = (TransitionPresenter)element;
            bool clip = (bool)value;
            if (!clip && transitionElement.IsTransitioning)
            {
                Transition transition = transitionElement.Transition;
                if (transition.ClipToBounds)
                {
                    return true;
                }
            }
            return value;
        }

        private void BeginTransition()
        {
            Transition transition = Transition;

            if (transition != null)
            {
                // Swap content presenters
                AdornerDecorator temp = m_previousHost;
                m_previousHost = m_currentHost;
                m_currentHost = temp;
            }

            ContentPresenter currentContent = CurrentContentPresenter;
            // Set the current content
            currentContent.Content = Content;
            currentContent.ContentTemplate = ContentTemplate;
            currentContent.ContentTemplateSelector = ContentTemplateSelector;

            if (transition != null)
            {
                ContentPresenter previousContent = PreviousContentPresenter;

                if (transition.IsNewContentTopmost)
                {
                    Children.Add(m_currentHost);
                }
                else
                {
                    Children.Insert(0, m_currentHost);
                }

                IsTransitioning = true;
                _activeTransition = transition;
                CoerceValue(TransitionProperty);
                CoerceValue(ClipToBoundsProperty);
                transition.BeginTransition(this, previousContent, currentContent);
            }
        }

        // Clean up after the transition is complete
        internal void OnTransitionCompleted()
        {
            m_children.Clear();
            m_children.Add(m_currentHost);
            m_currentHost.Visibility = Visibility.Visible;
            m_previousHost.Visibility = Visibility.Visible;
            ((ContentPresenter)m_previousHost.Child).Content = null;

            IsTransitioning = false;
            _activeTransition = null;
            CoerceValue(TransitionProperty);
            CoerceValue(ClipToBoundsProperty);
            CoerceValue(ContentProperty);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            m_currentHost.Measure(availableSize);
            return m_currentHost.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement uie in m_children)
            {
                uie.Arrange(new Rect(finalSize));
            }
            return finalSize;
        }

        protected override int VisualChildrenCount
        {
            get { return m_children.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= m_children.Count)
                throw new ArgumentOutOfRangeException("index");
            return m_children[index];
        }

        internal UIElementCollection Children
        {
            get { return m_children; }
        }

        private ContentPresenter PreviousContentPresenter
        {
            get { return ((ContentPresenter)m_previousHost.Child); }
        }

        private ContentPresenter CurrentContentPresenter
        {
            get { return ((ContentPresenter)m_currentHost.Child); }
        }

        #endregion
    }
}