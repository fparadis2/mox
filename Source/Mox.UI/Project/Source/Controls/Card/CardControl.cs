// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
    /// <summary>
    /// Card control.
    /// </summary>
    [TemplateVisualState(Name = IsTappedName, GroupName = TappedGroupName)]
    [TemplateVisualState(Name = IsNotTappedName, GroupName = TappedGroupName)]

    [TemplatePart(Name = GridName, Type = typeof(Grid))]
    public class CardControl : Button, IWeakEventListener
    {
        #region Constants

        private const string GridName = "PART_Grid";

        private const string TappedGroupName = "Tap";
        private const string IsTappedName = "IsTapped";
        private const string IsNotTappedName = "IsNotTapped";

        #endregion

        #region Variables

        public static readonly DependencyProperty ShowPowerAndToughnessProperty = DependencyProperty.Register("ShowPowerAndToughness", typeof(bool), typeof(CardControl), new PropertyMetadata(false));
        public static readonly DependencyProperty AnimateProperty = DependencyProperty.Register("Animate", typeof(bool), typeof(CardControl), new PropertyMetadata(true, OnAnimatePropertyChanged));

        private Grid m_grid;

        #endregion

        #region Constructor

        static CardControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CardControl), new FrameworkPropertyMetadata(typeof(CardControl)));
        }

        #endregion

        #region Properties

        public bool ShowPowerAndToughness
        {
            get { return (bool)GetValue(ShowPowerAndToughnessProperty); }
            set { SetValue(ShowPowerAndToughnessProperty, value); }
        }

        public Visual Frame
        {
            get { return m_grid; }
        }

        public bool Animate
        {
            get { return (bool)GetValue(AnimateProperty); }
            set { SetValue(AnimateProperty, value); }
        }

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            m_grid = (Grid)GetTemplateChild(GridName);
            Debug.Assert(m_grid != null);

            CardViewModel card = DataContext as CardViewModel;

            if (card != null)
            {
                GoToState(card, false);
                PropertyChangedEventManager.AddListener(card, this, string.Empty);
            }
        }

        public bool ReceiveWeakEvent(System.Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(PropertyChangedEventManager))
            {
                var ea = (System.ComponentModel.PropertyChangedEventArgs)e;
                if (string.Equals(ea.PropertyName, "Tapped"))
                {
                    GoToState((CardViewModel)sender, true);
                }

                return true;
            }

            return false;
        }

        private void GoToState(CardViewModel card, bool useTransitions)
        {
            VisualStateManager.GoToState(this, Animate && card.Tapped ? IsTappedName : IsNotTappedName, useTransitions);
        }

        #endregion

        #region Event Handlers

        private static void OnAnimatePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            CardControl cardControl = (CardControl)o;
            cardControl.GoToState(null, false);
        }

        #endregion
    }
}
