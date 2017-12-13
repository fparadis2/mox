using Mox.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Mox.UI
{
    public class SetIcon : Control
    {
        #region Dependency Properties

        public static readonly DependencyProperty SetProperty = DependencyProperty.Register("Set", typeof(string), typeof(SetIcon), new PropertyMetadata(null, OnSetChanged));

        public string Set
        {
            get { return (string)GetValue(SetProperty); }
            set { SetValue(SetProperty, value); }
        }

        public static readonly DependencyProperty RarityProperty = DependencyProperty.Register("Rarity", typeof(Rarity), typeof(SetIcon), new PropertyMetadata(Rarity.Common, OnRarityChanged));

        public Rarity Rarity
        {
            get { return (Rarity)GetValue(RarityProperty); }
            set { SetValue(RarityProperty, value); }
        }

        public static readonly DependencyProperty UseRarityGradientProperty = DependencyProperty.Register("UseRarityGradient", typeof(bool), typeof(SetIcon), new PropertyMetadata(false, OnRarityChanged));

        public bool UseRarityGradient
        {
            get { return (bool)GetValue(UseRarityGradientProperty); }
            set { SetValue(UseRarityGradientProperty, value); }
        }

        #endregion

        #region Constructor

        static SetIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SetIcon), new FrameworkPropertyMetadata(typeof(SetIcon)));
        }

        #endregion

        #region Event Handlers

        private static void OnSetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var setIdentifier = (string)e.NewValue ?? string.Empty;

            SetIcon icon = (SetIcon)d;
            icon.SetResourceReference(TemplateProperty, "Icon_Sets_" + setIdentifier.ToUpper());
        }

        private static void OnRarityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SetIcon icon = (SetIcon)d;

            string style = "SetIconStyle_" + icon.Rarity;

            if (icon.UseRarityGradient)
                style += "_Gradient";

            icon.SetResourceReference(StyleProperty, style);
        }

        #endregion
    }
}
