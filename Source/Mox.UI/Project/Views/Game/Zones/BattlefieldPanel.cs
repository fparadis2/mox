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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Mox.UI.Game
{
    /// <summary>
    /// A panel used to show the cards on the battlefield
    /// </summary>
    public class BattlefieldPanel : Panel
    {
        #region Variables

        private readonly Dictionary<object, UIElement> m_elementsByViewModel = new Dictionary<object, UIElement>();
        private readonly List<Size> m_lineSizes = new List<Size>();

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty RowSpacingProperty = DependencyProperty.Register("RowSpacing", typeof (double), typeof (BattlefieldPanel), new PropertyMetadata(0.1));

        public double RowSpacing
        {
            get { return (double) GetValue(RowSpacingProperty); }
            set { SetValue(RowSpacingProperty, value); }
        }

        public static readonly DependencyProperty GroupSpacingProperty = DependencyProperty.Register("GroupSpacing", typeof (double), typeof (BattlefieldPanel), new PropertyMetadata(0.1));

        public double GroupSpacing
        {
            get { return (double) GetValue(GroupSpacingProperty); }
            set { SetValue(GroupSpacingProperty, value); }
        }

        public static readonly DependencyProperty CardSpacingProperty = DependencyProperty.Register("CardSpacing", typeof(double), typeof(BattlefieldPanel), new PropertyMetadata(0.15));

        public double CardSpacing
        {
            get { return (double)GetValue(CardSpacingProperty); }
            set { SetValue(CardSpacingProperty, value); }
        }

        #endregion

        #region Constructor

        public BattlefieldPanel()
        {
            Background = Brushes.Transparent;
            Loaded += WhenLoaded;
        }

        #endregion

        #region Properties

        private double MinimumSlotWidth
        {
            get { return CardFrame.DefaultWidth; }
        }

        private double MinimumSlotHeight
        {
            get { return CardFrame.DefaultHeight; }
        }

        private BattlefieldViewModel Battlefield
        {
            get { return (BattlefieldViewModel)DataContext; }
        }

        #endregion

        #region Methods

        #region Binding

        private void WhenLoaded(object sender, RoutedEventArgs e)
        {
            var battlefield = Battlefield;
            WeakEventManager<BattlefieldViewModel, EventArgs>.AddHandler(battlefield, "ArrangeNeeded", WhenArrangeNeeded);
        }

        private void WhenArrangeNeeded(object sender, EventArgs e)
        {
            InvalidateArrange();
        }

        #endregion

        #region Arrangement

        private void RegisterElement(UIElement element)
        {
            FrameworkElement frameworkElement = element as FrameworkElement;
            if (frameworkElement != null)
            {
                var dataContext = frameworkElement.DataContext;
                if (dataContext != null)
                {
                    m_elementsByViewModel.Add(dataContext, element);
                }
            }
        }

        private struct BattlefieldLayout
        {
            public BattlefieldPanel Panel;
            public List<Size> LineSizes;

            public double Width;
            private double X, Y;
            private int CurrentRow;
            private double CurrentRowHeight;
            private double LastGroupElementWidth;

            private double RowSpacing
            {
                get { return Panel.MinimumSlotHeight * Panel.RowSpacing; }
            }

            private double GroupSpacing
            {
                get { return Panel.MinimumSlotWidth * Panel.GroupSpacing; }
            }

            private double CardSpacing
            {
                get { return Panel.MinimumSlotWidth * Panel.CardSpacing; }
            }

            public double Height
            {
                get { return Math.Max(0, Y - RowSpacing); }
            }

            public void End()
            {
                EndLine();
            }

            public bool ConsiderGroup(BattlefieldGroup group)
            {
                int row = GetRow(group.Type);
                Debug.Assert(row >= CurrentRow);

                if (row > CurrentRow)
                {
                    EndLine();
                    CurrentRow = row;
                    return true;
                }
                
                if (group.Count > 0)
                {
                    if (EndGroup())
                        X += GroupSpacing;
                }

                return false;
            }

            private static int GetRow(BattlefieldViewModel.PermanentType type)
            {
                switch (type)
                {
                    case BattlefieldViewModel.PermanentType.Creature:
                    case BattlefieldViewModel.PermanentType.Artifact:
                        return 0;

                    case BattlefieldViewModel.PermanentType.Land:
                    case BattlefieldViewModel.PermanentType.Planeswalker:
                    case BattlefieldViewModel.PermanentType.Enchantment:
                        return 1;

                    default:
                        throw new NotImplementedException();
                }
            }

            public Point ConsiderCard(CardViewModel card, UIElement element)
            {
                if (LastGroupElementWidth > 0)
                {
                    X += CardSpacing;
                }

                LastGroupElementWidth = Math.Max(Panel.MinimumSlotWidth, element.DesiredSize.Width);
                CurrentRowHeight = Math.Max(CurrentRowHeight, element.DesiredSize.Height);

                return new Point(X, Y);
            }

            private bool EndGroup()
            {
                if (LastGroupElementWidth > 0)
                {
                    X += LastGroupElementWidth;
                    LastGroupElementWidth = 0;
                    return true;
                }

                return false;
            }

            private void EndLine()
            {
                EndGroup();

                double lineWidth = X;
                double lineHeight = Math.Max(CurrentRowHeight, Panel.MinimumSlotHeight);

                if (LineSizes != null)
                {
                    LineSizes.Add(new Size(lineWidth, lineHeight));
                }

                Width = Math.Max(Width, lineWidth);

                X = 0;
                Y += lineHeight + RowSpacing;
                CurrentRowHeight = 0;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            m_elementsByViewModel.Clear();
            m_lineSizes.Clear();

            // Allow children as much room as they want - then scale them
            Size InfiniteSize = new Size(Double.PositiveInfinity, Double.PositiveInfinity);
            foreach (UIElement child in Children)
            {
                child.Measure(InfiniteSize);
                RegisterElement(child);
            }

            var battlefield = Battlefield;
            if (battlefield == null)
                return new Size(0, 0);

            var layout = new BattlefieldLayout { Panel = this, LineSizes = m_lineSizes };

            foreach (var group in battlefield.Groups)
            {
                layout.ConsiderGroup(group);

                foreach (var card in group)
                {
                    UIElement element;
                    if (m_elementsByViewModel.TryGetValue(card, out element))
                    {
                        layout.ConsiderCard(card, element);
                    }
                }
            }

            layout.End();

            return new Size(layout.Width, layout.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children == null || Children.Count == 0)
                return finalSize;

            if (m_lineSizes.Count <= 0)
                return finalSize;

            var battlefield = Battlefield;
            var layout = new BattlefieldLayout { Panel = this };

            Debug.Assert(m_lineSizes.Count > 0);

            int nextRow = 0;
            Size lineSize = m_lineSizes[nextRow++];
            double leftPadding = (finalSize.Width - lineSize.Width) / 2.0;

            int zIndex = 0;

            foreach (var group in battlefield.Groups)
            {
                if (layout.ConsiderGroup(group))
                {
                    lineSize = m_lineSizes[nextRow++];
                    leftPadding = (finalSize.Width - lineSize.Width) / 2.0;
                }

                foreach (var card in group)
                {
                    UIElement element;
                    if (m_elementsByViewModel.TryGetValue(card, out element))
                    {
                        var point = layout.ConsiderCard(card, element);

                        SetZIndex(element, zIndex++);
                        element.Arrange(new Rect(point.X + leftPadding, point.Y, element.DesiredSize.Width, element.DesiredSize.Height));
                    }
                }
            }

            layout.End();

            return new Size(layout.Width, layout.Height);
        }

        #endregion

        #endregion
    }
}
