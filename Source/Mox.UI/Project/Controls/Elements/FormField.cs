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

namespace Mox.UI
{
    public class FormField : HeaderedContentControl
    {
        static FormField()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FormField), new FrameworkPropertyMetadata(typeof(FormField)));
        }

        #region Dependency Properties

        #region Footer

        public static readonly DependencyProperty FooterProperty = DependencyProperty.Register(
            "Footer", typeof (object), typeof (FormField), new PropertyMetadata(default(object), OnFooterChanged));

        public object Footer
        {
            get { return (object) GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }

        public static readonly DependencyProperty FooterTemplateProperty = DependencyProperty.Register(
            "FooterTemplate", typeof (DataTemplate), typeof (FormField), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate FooterTemplate
        {
            get { return (DataTemplate) GetValue(FooterTemplateProperty); }
            set { SetValue(FooterTemplateProperty, value); }
        }

        private static readonly DependencyPropertyKey HasFooterPropertyKey = DependencyProperty.RegisterReadOnly(
            "HasFooter", typeof(bool), typeof(FormField), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty HasFooterProperty = HasFooterPropertyKey.DependencyProperty;

        public bool HasFooter
        {
            get { return (bool) GetValue(HasFooterProperty); }
        }

        private static void OnFooterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(HasFooterPropertyKey, e.NewValue != null);
        }

        #endregion

        #region Description

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description", typeof(object), typeof(FormField), new PropertyMetadata(default(object), OnDescriptionChanged));

        public object Description
        {
            get { return (object)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionTemplateProperty = DependencyProperty.Register(
            "DescriptionTemplate", typeof(DataTemplate), typeof(FormField), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate DescriptionTemplate
        {
            get { return (DataTemplate)GetValue(DescriptionTemplateProperty); }
            set { SetValue(DescriptionTemplateProperty, value); }
        }

        private static readonly DependencyPropertyKey HasDescriptionPropertyKey = DependencyProperty.RegisterReadOnly(
            "HasDescription", typeof(bool), typeof(FormField), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty HasDescriptionProperty = HasDescriptionPropertyKey.DependencyProperty;

        public bool HasDescription
        {
            get { return (bool)GetValue(HasDescriptionProperty); }
        }

        private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(HasDescriptionPropertyKey, e.NewValue != null);
        }

        #endregion

        #region MaximumDescriptionWidth

        public static readonly DependencyProperty MaximumDescriptionWidthProperty = DependencyProperty.Register(
            "MaximumDescriptionWidth", typeof (double), typeof (FormField), new PropertyMetadata(300.0));

        public double MaximumDescriptionWidth
        {
            get { return (double)GetValue(MaximumDescriptionWidthProperty); }
            set { SetValue(MaximumDescriptionWidthProperty, value); }
        }

        #endregion

        #endregion
    }
}
