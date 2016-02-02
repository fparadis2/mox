using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Mox.UI
{
    public class SymbolTextBlock : FrameworkElement
    {
        #region Variables

        private SymbolTextLayout m_layout = new SymbolTextLayout(null);

        #endregion

        #region Properties

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(SymbolTextBlock), 
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SymbolTextBlock)d).OnTextChanged((string)e.NewValue);
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(SymbolTextBlock));

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(SymbolTextBlock));

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(typeof(SymbolTextBlock));

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public static readonly DependencyProperty NewLineRatioProperty = DependencyProperty.Register(
            "NewLineRatio", typeof(double), typeof(SymbolTextBlock), new PropertyMetadata(1.0));

        public double NewLineRatio
        {
            get { return (double)GetValue(NewLineRatioProperty); }
            set { SetValue(NewLineRatioProperty, value); }
        }

        public static readonly DependencyProperty ItalicizeParenthesisProperty = DependencyProperty.Register(
            "ItalicizeParenthesis", typeof(bool), typeof(SymbolTextBlock), new PropertyMetadata(false));

        public bool ItalicizeParenthesis
        {
            get { return (bool)GetValue(ItalicizeParenthesisProperty); }
            set { SetValue(ItalicizeParenthesisProperty, value); }
        }

        #endregion

        #region Methods

        private void OnTextChanged(string newText)
        {
            m_layout = new SymbolTextLayout(newText);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            m_layout.Font = FontFamily;
            m_layout.FontSize = FontSize;
            m_layout.NewLineRatio = NewLineRatio;
            m_layout.ItalicizeParenthesis = ItalicizeParenthesis;

            m_layout.MaxSize = availableSize;

            return m_layout.RequiredSize;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var renderer = new SymbolTextRenderer(m_layout)
            {
                Brush = Foreground
            };

            renderer.Render(drawingContext, new Rect(new Point(), RenderSize));
        }

        #endregion
    }
}
