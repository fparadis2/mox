using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Caliburn.Micro;

namespace Mox.UI
{
    public class SymbolTextRenderer
    {
        #region Variables

        private readonly string m_text;
        private readonly ushort[] m_glyphIndices;
        private readonly double[] m_glyphAdvances;
        private readonly List<TextPart> m_parts = new List<TextPart>();
        private readonly List<double> m_lineWidths = new List<double>();

        private readonly Size m_maxSize;
        private readonly GlyphTypeface m_glyphTypeface;
        private readonly double m_fontSize;

        private const double SymbolPaddingFactor = 4.0 / 35.0;

	    #endregion

        #region Constructor

        public SymbolTextRenderer(string text, List<TextTokenizer.Token> tokens, Size maxSize, Typeface typeface, double fontSize)
        {
            Brush = Brushes.Black;

            m_text = text;
            m_maxSize = maxSize;
            m_fontSize = fontSize;

            if (!typeface.TryGetGlyphTypeface(out m_glyphTypeface))
            {
                throw new ArgumentException("Cannot get glyph typeface", "typeface");
            }

            if (text != null)
            {
                m_glyphIndices = new ushort[text.Length];
                m_glyphAdvances = new double[text.Length];
                PrepareGlyphs();
            }

            ParseTokens(tokens);
        }

        private static readonly Size InfiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

        public static SymbolTextRenderer Create(string text, FontFamily font, double fontSize)
        {
            List<TextTokenizer.Token> tokens = new List<TextTokenizer.Token>();
            TextTokenizer.Tokenize(text, tokens);

            var typeface = new Typeface(font, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            return new SymbolTextRenderer(text, tokens, InfiniteSize, typeface, fontSize);
        }
		 
	    #endregion

        #region Properties

        public Brush Brush { get; set; }

        public TextAlignment TextAlignment { get; set; }

        #endregion

        #region Methods

        #region Rendering

        public void Render(DrawingContext context, Rect bounds, double scale = 1)
        {
            if (m_parts.Count == 0)
                return;

            Transform(ref bounds, scale);

            //context.DrawRectangle(Brushes.Blue, null, bounds);

            int lineIndex = 0;

            Point origin = new Point { X = GetLineStartX(ref bounds, lineIndex, scale), Y = bounds.Top };

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

        public void Render(DrawingContext context, Point origin, double scale = 1)
        {
            if (m_parts.Count == 0)
                return;

            double left = origin.X;

            foreach (var part in m_parts)
            {
                RenderToken(context, origin, part, scale);
                origin.X += part.Width * scale;

                if (part.LineAdvance > 0)
                {
                    origin.X = left;
                    origin.Y += part.LineAdvance * scale;
                }
            }
        }

        private void RenderToken(DrawingContext context, Point origin, TextPart part, double scale)
        {
            object data = part.Data;
            if (ReferenceEquals(data, null))
            {
                RenderString(context, origin, part.StartIndex, part.EndIndex, part.Width * scale, scale);
                return;
            }

            ManaCost cost = data as ManaCost;
            if (cost != null)
            {
                RenderManaCost(context, cost, origin, scale);
                return;
            }

            MiscSymbols symbol = data as MiscSymbols;
            if (symbol != null)
            {
                RenderMiscSymbol(context, symbol, origin, scale);
            }
        }

        private void RenderString(DrawingContext context, Point origin, int start, int end, double width, double scale)
        {
            int count = end - start;
            var glyphIndices = new ushort[count];
            var glyphAdvances = new double[count];

            for (int n = 0; n < count; n++)
            {
                glyphIndices[n] = m_glyphIndices[start + n];
                glyphAdvances[n] = m_glyphAdvances[start + n] * scale;
            }

            //context.DrawRectangle(Brushes.Yellow, null, new Rect(origin, new Size(width, m_glyphTypeface.Height * m_fontSize * scale)));

            origin.Y += m_glyphTypeface.Baseline * m_fontSize * scale;
            GlyphRun run = new GlyphRun(m_glyphTypeface, 0, false, m_fontSize * scale, glyphIndices, origin, glyphAdvances, null, null, null, null, null, null);
            context.DrawGlyphRun(Brush, run);
        }

        private void RenderManaCost(DrawingContext context, ManaCost cost, Point origin, double scale)
        {
            double symbolSize = m_fontSize * scale;

            if (cost.Colorless > 0)
            {
                ImageKey colorless = ImageKey.ForManaSymbol(cost.Colorless);
                var image = ImageService.LoadImageSynchronous(colorless);
                context.DrawImage(image, new Rect(origin, new Size(symbolSize, symbolSize)));
                origin.X += (1 + SymbolPaddingFactor) * symbolSize;
            }

            foreach (var symbol in cost.Symbols)
            {
                ImageKey key = ImageKey.ForManaSymbol(symbol);
                var image = ImageService.LoadImageSynchronous(key);
                context.DrawImage(image, new Rect(origin, new Size(symbolSize, symbolSize)));
                origin.X += (1 + SymbolPaddingFactor) * symbolSize;
            }
        }

        private void RenderMiscSymbol(DrawingContext context, MiscSymbols symbol, Point origin, double scale)
        {
            double symbolSize = m_fontSize * scale;

            ImageKey key = ImageKey.ForMiscSymbol(symbol);
            var image = ImageService.LoadImageSynchronous(key);
            context.DrawImage(image, new Rect(origin, new Size(symbolSize, symbolSize)));
        }

        #endregion

        #region Measure

        private void PrepareGlyphs()
        {
            for (int i = 0; i < m_text.Length; i++)
            {
                char c = m_text[i];

                if (c == '\n')
                    continue;

                ushort glyphIndex = m_glyphTypeface.CharacterToGlyphMap[c];
                m_glyphIndices[i] = glyphIndex;
                m_glyphAdvances[i] = m_glyphTypeface.AdvanceWidths[glyphIndex] * m_fontSize;
            }
        }

        private void ParseTokens(List<TextTokenizer.Token> tokens)
        {
            if (tokens.Count == 0)
                return;

            ParserState state = new ParserState
            {
                LineAdvance = m_glyphTypeface.Height * m_fontSize,
                MaxLineWidth = m_maxSize.Width
            };
            state.Initialize(m_parts, m_lineWidths);

            foreach (var token in tokens)
            {
                ParseToken(ref state, token);
            }

            state.Finalize();

            Debug.Assert(m_lineWidths.Count > 0);
        }

        private void ParseToken(ref ParserState state, TextTokenizer.Token token)
        {
            if (token.Type == TextTokenizer.TokenType.NewLine)
            {
                state.AdvanceLine();
                return;
            }

            object data;
            double width = MeasureToken(ref token, out data);

            if (!ReferenceEquals(data, null))
            {
                // Custom data always means start a new part
                state.ConsiderCustomData(token.StartIndex, token.EndIndex, data, width);
                return;
            }

            state.ConsiderToken(ref token, width);
        }

        private double MeasureToken(ref TextTokenizer.Token token, out object data)
        {
            switch (token.Type)
            {
                case TextTokenizer.TokenType.Text:
                {
                    double specialTokenWidth = TryMeasureSpecialToken(token.StartIndex, token.EndIndex, out data);
                    if (specialTokenWidth > 0)
                        return specialTokenWidth;

                    break;
                }

                case TextTokenizer.TokenType.Separator:
                case TextTokenizer.TokenType.Whitespace:
                    break;
                default:
                    throw new InvalidProgramException();
            }

            data = null;
            return MeasureString(token.StartIndex, token.EndIndex);
        }

        private double MeasureString(int start, int end)
        {
            double width = 0;

            for (int index = start; index < end; index++)
            {
                width += m_glyphAdvances[index];
            }

            return width;
        }

        private double TryMeasureSpecialToken(int start, int end, out object data)
        {
            ManaCost cost;
            if (ManaCost.TryParse(m_text, start, end, ManaSymbolNotation.Long, out cost))
            {
                data = cost;
                int symbolCount = cost.SymbolCount;
                return (symbolCount + SymbolPaddingFactor * (symbolCount - 1)) * m_fontSize;
            }

            MiscSymbols miscSymbols;
            if (MiscSymbols.TryParse(m_text, start, end, out miscSymbols))
            {
                data = miscSymbols;
                return m_fontSize;
            }

            data = null;
            return 0;
        }

        #endregion

        #endregion

        #region Nested Types

        private struct TextPart
        {
            public object Data;

            public double LineAdvance;
            public double Width;

            public int StartIndex;
            public int EndIndex;
        }

        private struct ParserState
        {
            private List<TextPart> m_parts;
            private List<double> m_lineWidths;

            private TextPart m_currentPart;
            private TextPart m_tentativePart;
            private int m_tentativePartStartWhitespaceCount;
            private double m_tentativePartStartWhitespaceWidth;

            private double m_currentLineWidth;

            public double LineAdvance;
            public double MaxLineWidth;

            public void Initialize(List<TextPart> parts, List<double> lineWidths)
            {
                m_parts = parts;
                m_lineWidths = lineWidths;

                Reset(out m_currentPart);
                Reset(out m_tentativePart);
            }

            public void Finalize()
            {
                EndPart();
                m_lineWidths.Add(m_currentLineWidth); // Final line
            }

            public void AdvanceLine()
            {
                EndLineImpl();
                EndPart();
            }

            private void EndPart()
            {
                CommitTentativePart();

                if (m_currentPart.StartIndex >= 0)
                {
                    m_parts.Add(m_currentPart);
                    Reset(out m_currentPart);
                }
            }

            public void ConsiderToken(ref TextTokenizer.Token token, double width)
            {
                if (token.Type == TextTokenizer.TokenType.Whitespace)
                {
                    CommitTentativePart();
                }

                // Initialize part if needed
                if (m_tentativePart.StartIndex < 0)
                {
                    m_tentativePart.StartIndex = token.StartIndex;

                    if (token.Type == TextTokenizer.TokenType.Whitespace)
                    {
                        m_tentativePartStartWhitespaceCount = token.Length;
                        m_tentativePartStartWhitespaceWidth = width;
                    }
                }

                // Join the current token to the tentative part
                m_tentativePart.EndIndex = token.EndIndex;
                m_tentativePart.Width += width;
            }

            public void ConsiderCustomData(int start, int end, object data, double width)
            {
                if (m_currentLineWidth + width > MaxLineWidth && m_currentPart.StartIndex >= 0)
                {
                    EndLineImpl();
                }

                // Custom data always means start a new part
                EndPart();

                m_currentPart.StartIndex = start;
                m_currentPart.EndIndex = end;
                m_currentPart.Width = width;
                m_currentPart.Data = data;
                EndPart();

                m_currentLineWidth += m_tentativePart.Width;
            }

            private void CommitTentativePart()
            {
                if (m_tentativePart.StartIndex >= 0)
                {
                    if (m_currentLineWidth + m_tentativePart.Width > MaxLineWidth)
                    {
                        if (m_currentPart.StartIndex < 0)
                        {
                            m_currentPart.StartIndex = 0;
                            m_currentPart.EndIndex = 0;
                        }

                        // Not enough space, move tentative part to new line
                        EndLineImpl();

                        m_parts.Add(m_currentPart);
                        m_currentPart = m_tentativePart;
                        m_currentPart.StartIndex += m_tentativePartStartWhitespaceCount;
                        m_currentPart.Width -= m_tentativePartStartWhitespaceWidth;
                    }
                    else
                    {
                        if (m_currentPart.StartIndex < 0)
                        {
                            m_currentPart.StartIndex = m_tentativePart.StartIndex;
                        }

                        m_currentPart.EndIndex = m_tentativePart.EndIndex;
                        m_currentPart.Width += m_tentativePart.Width;
                    }

                    m_currentLineWidth += m_tentativePart.Width;
                    m_tentativePartStartWhitespaceCount = 0;
                    m_tentativePartStartWhitespaceWidth = 0;
                    Reset(out m_tentativePart);
                }
            }

            private void Reset(out TextPart part)
            {
                part = new TextPart { StartIndex = -1 };
            }

            private void EndLineImpl()
            {
                m_currentPart.LineAdvance = LineAdvance;
                m_lineWidths.Add(m_currentLineWidth);
                m_currentLineWidth = 0;
            }
        }
 
	    #endregion
    }
}
