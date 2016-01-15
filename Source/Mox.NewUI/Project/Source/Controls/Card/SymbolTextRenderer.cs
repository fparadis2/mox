using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Mox.UI
{
    public class SymbolTextLayout
    {
        #region Variables

        private readonly string m_text;
        private readonly List<TextTokenizer.Token> m_tokens = new List<TextTokenizer.Token>();

        // Result
        private SymbolTextRendererFont m_font;
        private SymbolTextRendererFont m_italicFont;

        private bool m_valid;
        private readonly List<SymbolTextPart> m_parts = new List<SymbolTextPart>();
        private readonly List<double> m_lineWidths = new List<double>();
        private double m_totalHeight;
		 
	    #endregion

        #region Constructor

        public SymbolTextLayout(string text)
        {
            m_text = text;
            TextTokenizer.Tokenize(text, m_tokens);
        }

        #endregion

        #region Properties

        public int LineCount
        {
            get
            {
                EnsureIsMeasured();
                return m_lineWidths.Count;
            }
        }

        private Size m_maxSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
        public Size MaxSize
        {
            get { return m_maxSize; }
            set
            {
                m_maxSize = value;
                m_valid = false;
            }
        }

        private double m_fontSize = 30;
        public double FontSize
        {
            get { return m_fontSize; }
            set
            {
                m_fontSize = value;
                m_valid = false;
            }
        }

        private Typeface m_typeface;
        public Typeface Typeface
        {
            get { return m_typeface; }
            set
            {
                m_typeface = value;
                m_valid = false;
            }
        }

        public FontFamily Font
        {
            get { return m_typeface.FontFamily; }
            set
            {
                if (m_typeface == null)
                    m_typeface = new Typeface(value, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
                else
                    m_typeface = new Typeface(value, m_typeface.Style, m_typeface.Weight, m_typeface.Stretch);

                m_valid = false;
            }
        }

        private double m_newLineRatio = 1.55;
        public double NewLineRatio
        {
            get { return m_newLineRatio; }
            set
            {
                m_newLineRatio = value;
                m_valid = false;
            }
        }

        private bool m_italicizeParenthesis;
        public bool ItalicizeParenthesis
        {
            get { return m_italicizeParenthesis; }
            set
            {
                m_italicizeParenthesis = value;
                m_valid = false;
            }
        }

        #endregion

        #region Methods

        internal void CreateRenderer(out List<SymbolTextPart> parts, out List<double> lineWidths, out double totalHeight)
        {
            EnsureIsMeasured();

            parts = m_parts;
            lineWidths = m_lineWidths;
            totalHeight = m_totalHeight;
        }

        private void EnsureIsMeasured()
        {
            if (m_valid)
                return;

            m_font = new SymbolTextRendererFont(m_text, m_typeface, m_fontSize);
            m_italicFont = null; // Created on demand
            m_parts.Clear();
            m_lineWidths.Clear();
            m_totalHeight = 0;

            Measure();
            m_valid = true;
        }

        private void Measure()
        {
            if (m_tokens.Count == 0)
                return;

            ParserState state = new ParserState { Parent = this };
            state.Initialize();

            foreach (var token in m_tokens)
            {
                MeasureToken(ref state, token);
            }

            state.FinalizeParsing();

            Debug.Assert(m_lineWidths.Count > 0);
            m_totalHeight = state.TotalHeight;
        }

        private void MeasureToken(ref ParserState state, TextTokenizer.Token token)
        {
            if (token.Type == TextTokenizer.TokenType.NewLine)
            {
                state.AdvanceLine(NewLineRatio);
                return;
            }

            if (token.Type == TextTokenizer.TokenType.Separator && ItalicizeParenthesis)
            {
                state.ConsiderSeparatorPreToken(m_text, ref token);
            }

            object data;
            double width = MeasureToken(state.CurrentFont, ref token, out data);

            if (!ReferenceEquals(data, null))
            {
                // Custom data always means start a new part
                state.ConsiderCustomData(token.StartIndex, token.EndIndex, data, width);
                return;
            }

            state.ConsiderToken(ref token, width);

            if (token.Type == TextTokenizer.TokenType.Separator && ItalicizeParenthesis)
            {
                state.ConsiderSeparatorPostToken(m_text, ref token);
            }
        }

        private double MeasureToken(SymbolTextRendererFont font, ref TextTokenizer.Token token, out object data)
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
            return font.MeasureString(token.StartIndex, token.EndIndex);
        }

        private double TryMeasureSpecialToken(int start, int end, out object data)
        {
            ManaCost cost;
            if (ManaCost.TryParse(m_text, start, end, ManaSymbolNotation.Long, out cost))
            {
                data = cost;
                int symbolCount = cost.SymbolCount;
                return (symbolCount + SymbolTextRenderer.SymbolPaddingFactor * (symbolCount - 1)) * m_font.FontSize;
            }

            MiscSymbols miscSymbols;
            if (MiscSymbols.TryParse(m_text, start, end, out miscSymbols))
            {
                data = miscSymbols;
                return m_font.FontSize;
            }

            data = null;
            return 0;
        }

        private SymbolTextRendererFont GetOrCreateItalicFont()
        {
            if (m_italicFont != null)
                return m_italicFont;

            var typeface = new Typeface(m_typeface.FontFamily, FontStyles.Italic, m_typeface.Weight, m_typeface.Stretch);
            m_italicFont = new SymbolTextRendererFont(m_text, typeface, m_font.FontSize);
            return m_italicFont;
        }

        #endregion

        #region ParserState

        private struct ParserState
        {
            private SymbolTextPart m_currentPart;
            private SymbolTextPart m_tentativePart;
            private int m_tentativePartStartWhitespaceCount;
            private double m_tentativePartStartWhitespaceWidth;

            private double m_currentLineWidth;

            private int m_italicScopeCount;

            internal SymbolTextLayout Parent;

            private double MaxLineWidth { get { return Parent.m_maxSize.Width; } }

            public double TotalHeight { get; private set; }

            public SymbolTextRendererFont CurrentFont
            {
                get
                {
                    Debug.Assert(m_currentPart.Font != null);
                    return m_currentPart.Font;
                }
            }

            public void Initialize()
            {
                Reset(out m_currentPart);
                Reset(out m_tentativePart);
            }

            public void FinalizeParsing()
            {
                EndPart();
                Parent.m_lineWidths.Add(m_currentLineWidth); // Final line
                TotalHeight += Parent.m_font.LineHeight;
            }

            public void AdvanceLine(double factor = 1)
            {
                CommitTentativePart();
                EndLineImpl(factor);
                EndPart();
            }

            private void EndPart()
            {
                CommitTentativePart();

                if (m_currentPart.StartIndex >= 0)
                {
                    Parent.m_parts.Add(m_currentPart);
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

                m_currentLineWidth += width;
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

                        Parent.m_parts.Add(m_currentPart);
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

            private void Reset(out SymbolTextPart part)
            {
                part = new SymbolTextPart { StartIndex = -1, Font = Parent.m_font };

                if (m_italicScopeCount > 0)
                {
                    part.Font = Parent.GetOrCreateItalicFont();
                }
            }

            private void EndLineImpl(double factor = 1)
            {
                if (m_currentPart.StartIndex < 0)
                {
                    m_currentPart.StartIndex = 0;
                    m_currentPart.EndIndex = 0;
                }

                double advance = m_currentPart.Font.LineHeight * factor;
                TotalHeight += advance;
                m_currentPart.LineAdvance = advance;
                Parent.m_lineWidths.Add(m_currentLineWidth);
                m_currentLineWidth = 0;
            }

            public void ConsiderSeparatorPreToken(string text, ref TextTokenizer.Token token)
            {
                for (int i = token.StartIndex; i < token.EndIndex; i++)
                {
                    if (text[i] == '(')
                    {
                        if (m_italicScopeCount++ == 0)
                        {
                            // Entering an italic section
                            EndPart();
                        }
                    }
                }
            }

            public void ConsiderSeparatorPostToken(string text, ref TextTokenizer.Token token)
            {
                for (int i = token.StartIndex; i < token.EndIndex; i++)
                {
                    if (text[i] == ')')
                    {
                        if (--m_italicScopeCount == 0)
                        {
                            // Exiting an italic section
                            EndPart();
                        }
                    }
                }
            }
        }

        #endregion
    }

    public class SymbolTextRenderer
    {
        #region Constants

        internal const double SymbolPaddingFactor = 4.0 / 35.0;

        #endregion

        #region Variables

        private readonly List<SymbolTextPart> m_parts;
        private readonly List<double> m_lineWidths;
        private readonly double m_totalHeight;

	    #endregion

        #region Constructor

        public SymbolTextRenderer(SymbolTextLayout layout)
        {
            Brush = Brushes.Black;

            layout.CreateRenderer(out m_parts, out m_lineWidths, out m_totalHeight);
        }
		 
	    #endregion

        #region Properties

        public Brush Brush { get; set; }

        public TextAlignment TextAlignment { get; set; }
        public VerticalAlignment VerticalAlignment { get; set; }

        public bool RenderSymbolShadows { get; set; }

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

            /*GuidelineSet guidelines = new GuidelineSet();
            guidelines.GuidelinesY.Add(origin.Y + m_glyphTypeface.Baseline * m_fontSize * scale);
            guidelines.Freeze();
            context.PushGuidelineSet(guidelines);*/

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

            GlyphRun run = part.Font.CreateGlyphRun(origin, part.StartIndex, part.EndIndex, scale);

            //var bounds = run.ComputeAlignmentBox();
            //bounds.X += origin.X;
            //bounds.Y += origin.Y;
            //context.DrawRectangle(null, new Pen(Brushes.Red, 1), bounds);

            context.DrawGlyphRun(Brush, run);
        }

        private void RenderManaCost(DrawingContext context, ManaCost cost, Point origin, ref SymbolTextPart part, double scale)
        {
            if (cost.Colorless > 0)
            {
                ImageKey key = ImageKey.ForManaSymbol(cost.Colorless);
                RenderSymbol(context, key, ref origin, ref part, scale);
            }

            foreach (var symbol in cost.Symbols)
            {
                ImageKey key = ImageKey.ForManaSymbol(symbol);
                RenderSymbol(context, key, ref origin, ref part, scale);
            }
        }

        private void RenderMiscSymbol(DrawingContext context, MiscSymbols symbol, Point origin, ref SymbolTextPart part, double scale)
        {
            ImageKey key = ImageKey.ForMiscSymbol(symbol);
            RenderSymbol(context, key, ref origin, ref part, scale);
        }

        private void RenderSymbol(DrawingContext context, ImageKey key, ref Point origin, ref SymbolTextPart part, double scale)
        {
            double symbolSize = part.Font.FontSize * scale;

            if (RenderSymbolShadows)
            {
                double shadowSize = symbolSize * 1.057;
                double difference = shadowSize - symbolSize;

                var shadow = ImageService.LoadImageSynchronous(ImageKey.ForMiscSymbol(MiscSymbols.SymbolShadow));
                context.DrawImage(shadow, new Rect(origin + new Vector(-difference, difference), new Size(shadowSize, shadowSize)));
            }
            
            var image = ImageService.LoadImageSynchronous(key);
            context.DrawImage(image, new Rect(origin, new Size(symbolSize, symbolSize)));

            origin.X += (1 + SymbolPaddingFactor) * symbolSize;
        }

        #endregion
    }

    internal struct SymbolTextPart
    {
        public object Data;

        public SymbolTextRendererFont Font;

        public double LineAdvance;
        public double Width;

        public int StartIndex;
        public int EndIndex;
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

        public double LineHeight
        {
            get { return m_glyphTypeface.Baseline * m_fontSize; }
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
