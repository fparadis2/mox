using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mox.UI
{
    public class SymbolTextRenderer
    {
        #region Constants

        internal const double SymbolPaddingFactor = 4.0 / 35.0;

        #endregion

        #region Variables

        private readonly List<SymbolTextPart> m_parts;
        private readonly List<double> m_lineWidths;
        private readonly double m_totalHeight;
        private readonly double m_hybridManaScaleFactor;

	    #endregion

        #region Constructor

        public SymbolTextRenderer(SymbolTextLayout layout)
        {
            Brush = Brushes.Black;

            layout.CreateRenderer(out m_parts, out m_lineWidths, out m_totalHeight);
            m_hybridManaScaleFactor = layout.HybridManaScaleFactor;
        }
		 
	    #endregion

        #region Properties

        public Brush Brush { get; set; }

        public TextAlignment TextAlignment { get; set; }
        public VerticalAlignment VerticalAlignment { get; set; }

        public bool RenderSymbolShadows { get; set; }

        public double TotalHeight
        {
            get { return m_totalHeight; }
        }

        #endregion

        #region Methods

        public void Render(DrawingContext context, Rect bounds, double scale = 1)
        {
            if (m_parts.Count == 0)
                return;

            Transform(ref bounds, scale);

            //context.DrawRectangle(Brushes.Blue, null, bounds);

            int lineIndex = 0;

            Point origin = new Point { X = GetLineStartX(ref bounds, lineIndex, scale), Y = GetLineStartY(ref bounds, scale) };

            for (int i = 0; i < m_parts.Count; i++)
            {
                var part = m_parts[i];
                RenderToken(context, origin, part, scale);
                origin.X += part.Width * scale;

                if (i < m_parts.Count - 1 && part.LineAdvance > 0)
                {
                    lineIndex++;
                    origin.X = GetLineStartX(ref bounds, lineIndex, scale);
                    origin.Y += part.LineAdvance * scale;
                }
            }
        }

        private double GetLineStartX(ref Rect bounds, int lineIndex, double scale)
        {
            switch (TextAlignment)
            {
                case TextAlignment.Left:
                    return bounds.Left;

                case TextAlignment.Center:
                    return (bounds.Left + bounds.Right - m_lineWidths[lineIndex] * scale) / 2.0;

                case TextAlignment.Right:
                    return bounds.Right - m_lineWidths[lineIndex] * scale;

                default:
                    throw new NotImplementedException();
            }
        }

        private double GetLineStartY(ref Rect bounds, double scale)
        {
            switch (VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    return bounds.Top;

                case VerticalAlignment.Center:
                case VerticalAlignment.Stretch:
                    return (bounds.Top + bounds.Bottom - m_totalHeight * scale) / 2.0;

                case VerticalAlignment.Bottom:
                    return bounds.Bottom - m_totalHeight * scale;

                default:
                    throw new NotImplementedException();
            }
        }

        private static void Transform(ref Rect rect, double scale)
        {
            rect.X *= scale;
            rect.Y *= scale;

            rect.Width *= scale;
            rect.Height *= scale;
        }

        private void RenderToken(DrawingContext context, Point origin, SymbolTextPart part, double scale)
        {
            object data = part.Data;
            if (ReferenceEquals(data, null))
            {
                RenderString(context, origin, ref part, scale);
                return;
            }

            ManaCost cost = data as ManaCost;
            if (cost != null)
            {
                RenderManaCost(context, cost, origin, ref part, scale);
                return;
            }

            MiscSymbols symbol = data as MiscSymbols;
            if (symbol != null)
            {
                RenderMiscSymbol(context, symbol, origin, ref part, scale);
            }
        }

        private void RenderString(DrawingContext context, Point origin, ref SymbolTextPart part, double scale)
        {
            if (part.StartIndex >= part.EndIndex)
                return;

            GuidelineSet guidelines = new GuidelineSet();
            guidelines.GuidelinesY.Add(origin.Y + part.Font.BaseLine * scale);
            guidelines.Freeze();
            context.PushGuidelineSet(guidelines);

            GlyphRun run = part.Font.CreateGlyphRun(origin, part.StartIndex, part.EndIndex, scale);
            context.DrawGlyphRun(Brush, run);

            context.Pop();
        }

        private void RenderManaCost(DrawingContext context, ManaCost cost, Point origin, ref SymbolTextPart part, double scale)
        {
            if (cost.IsEmpty)
            {
                ImageKey key = ImageKey.ForManaSymbol(0);
                var symbolSize = part.Font.BaseSymbolSize;
                RenderSymbol(context, key, ref origin, ref part, symbolSize, scale);
                return;
            }

            if (cost.Colorless > 0)
            {
                ImageKey key = ImageKey.ForManaSymbol(cost.Colorless);
                var symbolSize = part.Font.BaseSymbolSize;
                RenderSymbol(context, key, ref origin, ref part, symbolSize, scale);
            }

            foreach (var symbol in cost.Symbols)
            {
                ImageKey key = ImageKey.ForManaSymbol(symbol);
                var symbolSize = part.Font.GetSymbolSize(symbol, m_hybridManaScaleFactor);
                RenderSymbol(context, key, ref origin, ref part, symbolSize, scale);
            }
        }

        private void RenderMiscSymbol(DrawingContext context, MiscSymbols symbol, Point origin, ref SymbolTextPart part, double scale)
        {
            ImageKey key = ImageKey.ForMiscSymbol(symbol);
            var symbolSize = part.Font.GetSymbolSize(symbol);
            RenderSymbol(context, key, ref origin, ref part, symbolSize, scale);
        }

        private void RenderSymbol(DrawingContext context, ImageKey key, ref Point origin, ref SymbolTextPart part, double symbolSize, double scale)
        {
            double symbolHeight = symbolSize * scale;
            double lineHeight = part.Font.LineHeight * scale;

            double yOffset = (lineHeight - symbolHeight);

            var image = ImageService.LoadImageSynchronous(key);
            double symbolWidth = symbolHeight * image.Width / image.Height;

            if (RenderSymbolShadows)
            {
                double shadowSize = symbolHeight * 1.057; // Assumes square symbols
                double difference = shadowSize - symbolHeight;

                var shadow = ImageService.LoadImageSynchronous(ImageKey.ForMiscSymbol(MiscSymbols.SymbolShadow));
                context.DrawImage(shadow, new Rect(origin + new Vector(0, yOffset) + new Vector(-difference, difference), new Size(shadowSize, shadowSize)));
            }

            //context.DrawRectangle(null, new Pen(Brushes.DarkGreen, 1), new Rect(origin, new Size(symbolSize, lineHeight)));
            //context.DrawRectangle(null, new Pen(Brushes.Blue, 1), new Rect(origin + new Vector(0, yOffset), new Size(symbolSize, symbolSize)));

            /*GuidelineSet guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(origin.X);
            guidelines.GuidelinesY.Add(origin.Y + yOffset);
            guidelines.Freeze();
            context.PushGuidelineSet(guidelines);*/

            context.DrawImage(image, new Rect(origin + new Vector(0, yOffset), new Size(symbolWidth, symbolHeight)));

            origin.X += (1 + SymbolPaddingFactor) * symbolHeight;

            //context.Pop();
        }

        #endregion
    }

    internal class SymbolTextRendererFont
    {
        private readonly GlyphTypeface m_glyphTypeface;
        private readonly double m_fontSize;
        private readonly ushort[] m_glyphIndices;
        private readonly double[] m_glyphAdvances;

        public SymbolTextRendererFont(string text, Typeface typeface, double fontSize)
        {
            m_fontSize = fontSize;

            if (!typeface.TryGetGlyphTypeface(out m_glyphTypeface))
            {
                throw new ArgumentException("Cannot get glyph typeface", "typeface");
            }

            if (text != null)
            {
                m_glyphIndices = new ushort[text.Length];
                m_glyphAdvances = new double[text.Length];
                PrepareGlyphs(text, fontSize);
            }
        }

        public double FontSize
        {
            get { return m_fontSize; }
        }

        public double BaseLine
        {
            get { return m_glyphTypeface.Baseline * m_fontSize; }
        }

        public double LineHeight
        {
            get { return m_glyphTypeface.Baseline * m_fontSize * 1.1; }
        }

        public double BaseSymbolSize
        {
            get { return m_glyphTypeface.Baseline * m_fontSize * 0.85; }
        }

        public double GetSymbolSize(ManaSymbol symbol, double hybridManaScaleFactor)
        {
            double symbolSize = BaseSymbolSize;

            if (ManaSymbolHelper.IsHybrid(symbol))
            {
                symbolSize *= hybridManaScaleFactor;
            }

            return symbolSize;
        }

        public double GetSymbolSize(MiscSymbols symbol)
        {
            return BaseSymbolSize;
        }

        private void PrepareGlyphs(string text, double fontSize)
        {
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\n')
                    continue;

                ushort glyphIndex = m_glyphTypeface.CharacterToGlyphMap[c];
                m_glyphIndices[i] = glyphIndex;
                m_glyphAdvances[i] = m_glyphTypeface.AdvanceWidths[glyphIndex] * fontSize;
            }
        }

        public GlyphRun CreateGlyphRun(Point origin, int start, int end, double scale)
        {
            int count = end - start;
            var glyphIndices = new ushort[count];
            var glyphAdvances = new double[count];

            for (int n = 0; n < count; n++)
            {
                glyphIndices[n] = m_glyphIndices[start + n];
                glyphAdvances[n] = m_glyphAdvances[start + n] * scale;
            }

            origin.Y += m_glyphTypeface.Baseline * m_fontSize * scale;
            return new GlyphRun(m_glyphTypeface, 0, false, m_fontSize * scale, glyphIndices, origin, glyphAdvances, null, null, null, null, null, null);
        }

        public double MeasureString(int start, int end)
        {
            double width = 0;

            for (int index = start; index < end; index++)
            {
                width += m_glyphAdvances[index];
            }

            return width;
        }
    }
}
