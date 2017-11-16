using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mox.UI
{
    /// <summary>
    /// Interaction logic for DialogOverlayView.xaml
    /// </summary>
    public partial class DialogOverlayView : UserControl
    {
        public static readonly DependencyProperty MainContentProperty = DependencyProperty.Register("MainContent", typeof(UIElement), typeof(DialogOverlayView));

        public UIElement MainContent
        {
            get { return (UIElement)GetValue(MainContentProperty); }
            set { SetValue(MainContentProperty, value); }
        }

        private static readonly TimeSpan AnimationDuration = TimeSpan.FromMilliseconds(200);

        static DialogOverlayView()
        {
            VisibilityProperty.AddOwner(typeof(DialogOverlayView), new FrameworkPropertyMetadata(
                    Visibility.Collapsed,
                    VisibilityChanged,
                    CoerceVisibility));
        }

        public DialogOverlayView()
        {
            InitializeComponent();
        }
        
        private static void VisibilityChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            // Ignore
        }

        private bool m_goingVisible = false;

        private static object CoerceVisibility(DependencyObject dependencyObject, object baseValue)
        {
            DialogOverlayView overlay = (DialogOverlayView)dependencyObject;
            Visibility visibility = (Visibility)baseValue;
            
            if (visibility == overlay.Visibility)
                return baseValue; // No change

            bool goingVisible = visibility == Visibility.Visible;
            if (overlay.m_goingVisible == goingVisible)
                return baseValue;

            overlay.m_goingVisible = goingVisible;

            var content = overlay.MainContent;

            var opacityAnimation = MakeAnimation(goingVisible ? 1.0f : 0.0, AnimationDuration);
            opacityAnimation.Completed += (sender, eventArgs) =>
            {
                if (!goingVisible)
                {
                    if (BindingOperations.IsDataBound(overlay, VisibilityProperty))
                    {
                        Binding bindingValue = BindingOperations.GetBinding(overlay, VisibilityProperty);
                        BindingOperations.SetBinding(overlay, VisibilityProperty, bindingValue);
                    }
                    else
                    {
                        // No binding, just assign the value
                        overlay.Visibility = visibility;
                    }

                    if (content != null)
                        content.Effect = null;
                }
            };

            overlay.BeginAnimation(OpacityProperty, opacityAnimation);

            if (content != null)
            {
                var effect = content.Effect as BlurEffect;
                if (effect == null)
                {
                    effect = new BlurEffect { Radius = 0 };
                    content.Effect = effect;
                }

                var effectAnimation = MakeAnimation(goingVisible ? 20.0f : 0.0f, AnimationDuration);
                effect.BeginAnimation(BlurEffect.RadiusProperty, effectAnimation);
            }

            // Remain visible for the duration of the animation
            return Visibility.Visible;
        }

        private static DoubleAnimation MakeAnimation(double to, TimeSpan duration)
        {
            DoubleAnimation anim = new DoubleAnimation(to, duration)
            {
                AccelerationRatio = 0.3,
                DecelerationRatio = 0.7
            };

            return anim;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            var source = e.OriginalSource as FrameworkElement;
            if (source == null)
                return;

            if (!source.IsChildOf(_Content))
            {
                DialogConductor conductor = DataContext as DialogConductor;
                if (conductor != null)
                {
                    conductor.Close();
                }
            }
        }
    }
}
