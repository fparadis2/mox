using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        private double m_maxWidth;
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

        public Size RequiredSize
        {
            get
            {
                EnsureIsMeasured();
                return new Size(m_maxWidth, m_totalHeight);
            }
        }

        public double Height
        {
            get
            {
                EnsureIsMeasured();
                return m_totalHeight;
            }
        }

        private Size m_maxSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
        public Size MaxSize
        {
            get { return m_maxSize; }
            set
            {
                if (m_maxSize != value)
                {
                    m_maxSize = value;
                    m_valid = false;
                }
            }
        }

        private double m_fontSize = 30;
        public double FontSize
        {
            get { return m_fontSize; }
            set
            {
                if (m_fontSize != value)
                {
                    m_fontSize = value;
                    m_valid = false;
                }
            }
        }

        private Typeface m_typeface;
        public Typeface Typeface
        {
            get { return m_typeface; }
            set
            {
                if (!Equals(m_typeface, value))
                {
                    m_typeface = value;
                    m_valid = false;
                }
            }
        }

        public FontFamily Font
        {
            get { return m_typeface.FontFamily; }
            set
            {
                if (m_typeface == null || m_typeface.FontFamily != value)
                {
                    if (m_typeface == null)
                        m_typeface = new Typeface(value, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
                    else
                        m_typeface = new Typeface(value, m_typeface.Style, m_typeface.Weight, m_typeface.Stretch);

                    m_valid = false;
                }
            }
        }

        public FontStyle FontStyle
        {
            get { return m_typeface.Style; }
            set
            {
                if (m_typeface == null || m_typeface.Style != value)
                {
                    if (m_typeface == null)
                        m_typeface = new Typeface(new FontFamily(), value, FontWeights.Normal, FontStretches.Normal);
                    else
                        m_typeface = new Typeface(m_typeface.FontFamily, value, m_typeface.Weight, m_typeface.Stretch);

                    m_valid = false;
                }
            }
        }

        private double m_newLineRatio = 1.55;
        public double NewLineRatio
        {
            get { return m_newLineRatio; }
            set
            {
                if (m_newLineRatio != value)
                {
                    m_newLineRatio = value;
                    m_valid = false;
                }
            }
        }

        public double LineHeight
        {
            get
            {
                return m_font.LineHeight;
            }
        }

        private double m_lineHeightFactor = 0.82;
        public double LineHeightFactor
        {
            get { return m_lineHeightFactor; }
            set
            {
                if (m_lineHeightFactor != value)
                {
                    m_lineHeightFactor = value;
                    m_valid = false;
                }
            }
        }

        private bool m_italicizeParenthesis;
        public bool ItalicizeParenthesis
        {
            get { return m_italicizeParenthesis; }
            set
            {
                if (m_italicizeParenthesis != value)
                {
                    m_italicizeParenthesis = value;
                    m_valid = false;
                }
            }
        }

        private double m_hybridManaScaleFactor = 1.25;
        public double HybridManaScaleFactor
        {
            get { return m_hybridManaScaleFactor; }
            set
            {
                if (m_hybridManaScaleFactor != value)
                {
                    m_hybridManaScaleFactor = value;
                    m_valid = false;
                }
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

            m_font = new SymbolTextRendererFont(m_text, m_typeface, m_fontSize) { LineHeightFactor = m_lineHeightFactor };
            m_italicFont = null; // Created on demand
            m_parts.Clear();
            m_lineWidths.Clear();

            m_maxWidth = 0;
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
            m_maxWidth = m_lineWidths.Max();
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

                if (cost.IsEmpty)
                {
                    return m_font.BaseSymbolSize;
                }

                double width = 0;

                if (cost.Generic > 0)
                {
                    width += m_font.BaseSymbolSize;
                }

                foreach (var symbol in cost.Symbols)
                {
                    width += m_font.GetSymbolSize(symbol, m_hybridManaScaleFactor);
                }

                width += m_font.BaseSymbolSize * SymbolTextRenderer.SymbolPaddingFactor * (cost.SymbolCount - 1);
                return width;
            }

            MiscSymbols miscSymbols;
            if (MiscSymbols.TryParse(m_text, start, end, out miscSymbols))
            {
                data = miscSymbols;
                return m_font.GetSymbolSize(miscSymbols);
            }

            data = null;
            return 0;
        }

        private SymbolTextRendererFont GetOrCreateItalicFont()
        {
            if (m_italicFont != null)
                return m_italicFont;

            var typeface = new Typeface(m_typeface.FontFamily, FontStyles.Italic, m_typeface.Weight, m_typeface.Stretch);
            m_italicFont = new SymbolTextRendererFont(m_text, typeface, m_font.FontSize) { LineHeightFactor = m_lineHeightFactor };
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

            public void AdvanceLine(double factor)
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

    internal struct SymbolTextPart
    {
        public object Data;

        public SymbolTextRendererFont Font;

        public double LineAdvance;
        public double Width;

        public int StartIndex;
        public int EndIndex;
    }
}