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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mox.UI.Game
{
    public class CardInteractionFeedbackControl : Control
    {
        static CardInteractionFeedbackControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CardInteractionFeedbackControl), new FrameworkPropertyMetadata(typeof(CardInteractionFeedbackControl)));
        }

        public static readonly DependencyProperty InteractionTypeProperty = DependencyProperty.Register("InteractionType", typeof(InteractionType), typeof(CardInteractionFeedbackControl), new PropertyMetadata(default(InteractionType)));

        public InteractionType InteractionType
        {
            get { return (InteractionType)GetValue(InteractionTypeProperty); }
            set { SetValue(InteractionTypeProperty, value); }
        }

        public static readonly DependencyProperty IsHoveredProperty = DependencyProperty.Register("IsHovered", typeof (bool), typeof (CardInteractionFeedbackControl), new PropertyMetadata(default(bool)));

        public bool IsHovered
        {
            get { return (bool) GetValue(IsHoveredProperty); }
            set { SetValue(IsHoveredProperty, value); }
        }

        public static readonly DependencyProperty VignetteColorProperty = DependencyProperty.Register(
            "VignetteColor", typeof (System.Windows.Media.Color), typeof (CardInteractionFeedbackControl), new PropertyMetadata(default(System.Windows.Media.Color)));

        public System.Windows.Media.Color VignetteColor
        {
            get { return (System.Windows.Media.Color) GetValue(VignetteColorProperty); }
            set { SetValue(VignetteColorProperty, value); }
        }

        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register("Radius", typeof (double), typeof (CardInteractionFeedbackControl), new PropertyMetadata(0.47));

        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }
    }
}
