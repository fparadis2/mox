using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Media;

namespace Mox.UI
{
    public class SymbolTextRenderer
    {
        #region Variables

        private readonly List<object> m_tokens;

        private readonly List<TextPart> m_partsCache;
        private double m_maxLineWidth;

	    #endregion

        #region Constructor

        public SymbolTextRenderer(string text)
        {
            var tokens = SymbolTextTokenizer.Tokenize(text, ManaSymbolNotation.Long);
            m_tokens = new List<object>(tokens);
            m_partsCache = new List<TextPart>(m_tokens.Count);

            Brush = Brushes.Black;
        }
		 
	    #endregion

        #region Properties

        private Size m_maxSize;
        public Size MaxSize
        {
            get { return m_maxSize; }
            set
            {
                if (!(EqualsWithEpsilon(m_maxSize.Width, value.Width)) ||
                    !(EqualsWithEpsilon(m_maxSize.Height, value.Height)))
                {
                    m_maxSize = value;
                    InvalidatePartCache();
                }
            }
        }

        private Typeface m_typeface;
        private GlyphTypeface m_glyphTypeface;
        public Typeface Typeface
        {
            get { return m_typeface; }
            set
            {
                m_typeface = value;
                bool hasGlyphTypeface = m_typeface.TryGetGlyphTypeface(out m_glyphTypeface);
                Debug.Assert(hasGlyphTypeface);
                InvalidatePartCache();
            }
        }

        private double m_fontSize = 12;
        public double FontSize
        {
            get { return m_fontSize; }
            set
            {
                if (!(EqualsWithEpsilon(m_fontSize, value)))
                {
                    m_fontSize = value;
                    InvalidatePartCache();
                }
            }
        }

        public Brush Brush { get; set; }

        #endregion

        #region Methods

        #region Rendering

        public void Render(DrawingContext context, Point origin)
        {
            if (m_tokens.Count == 0)
                return;

            UpdateTokenCache();

            double left = origin.X;

            foreach (var part in m_partsCache)
            {
                if (part.LineAdvance > 0)
                {
                    origin.X = left;
                    origin.Y += part.LineAdvance;
                }

                RenderToken(context, origin, part);
                origin.X += part.Width;
            }
        }

        private void RenderToken(DrawingContext context, Point origin, TextPart part)
        {
            var token = part.Token;
            if (token is string)
            {
                RenderString(context, origin, (string)token, ref part);
            }
        }

        private void RenderString(DrawingContext context, Point origin, string token, ref TextPart part)
        {
            int count = part.EndIndex - part.StartIndex;
            var glyphIndices = new ushort[count];
            var glyphAdvances = new double[count];

            for (int n = 0; n < count; n++)
            {
                ushort glyphIndex = m_glyphTypeface.CharacterToGlyphMap[token[part.StartIndex + n]];
                glyphIndices[n] = glyphIndex;

                double advance = m_glyphTypeface.AdvanceWidths[glyphIndex] * m_fontSize;
                glyphAdvances[n] = advance;
            }

            origin.Y += m_glyphTypeface.Baseline * FontSize;
            GlyphRun run = new GlyphRun(m_glyphTypeface, 0, false, m_fontSize, glyphIndices, origin, glyphAdvances, null, null, null, null, null, null);
            context.DrawGlyphRun(Brush, run);
        }

        #endregion

        #region Measure

        public Size GetSize()
        {
            if (m_tokens.Count == 0)
                return Size.Empty;

            UpdateTokenCache();

            // todo
            return Size.Empty;
        }

        private void UpdateTokenCache()
        {
            if (m_partsCache.Count > 0)
                return;

            m_maxLineWidth = 0;

            double remainingWidth;
            AdvanceLine(out remainingWidth);

            foreach (var token in m_tokens)
            {
                TextPart lastPart;
                if (MeasureToken(token, ref remainingWidth, out lastPart))
                {
                    if (remainingWidth < 0)
                    {
                        double lineWidth = m_maxSize.Width - remainingWidth - lastPart.Width;
                        lastPart.LineAdvance = AdvanceLine(out remainingWidth);

                        m_maxLineWidth = Math.Max(m_maxLineWidth, lineWidth);
                    }
                }
            }
        }

        private void InvalidatePartCache()
        {
            m_partsCache.Clear();
        }

        private bool MeasureToken(object token, ref double remainingWidth, out TextPart lastPart)
        {
            if (token is string)
            {
                MeasureString((string) token, ref remainingWidth, out lastPart);
                return true;
            }

            lastPart = new TextPart();
            return false;
        }

        private void MeasureString(string tokenString, ref double remainingWidth, out TextPart lastPart)
        {
            int index = 0;
            lastPart = new TextPart();
            double nextLineAdvance = 0;

            while (index < tokenString.Length)
            {
                TextPart part;
                if (!MeasureStringImpl(tokenString, index, remainingWidth, out part))
                    break;

                remainingWidth -= part.Width;
                lastPart = part;

                part.LineAdvance = nextLineAdvance;

                if (index != tokenString.Length)
                {
                    nextLineAdvance = AdvanceLine(out remainingWidth);
                }

                index = part.EndIndex;
                m_partsCache.Add(part);
            }

            Debug.Assert(ReferenceEquals(lastPart.Token, tokenString));
        }

        private bool MeasureStringImpl(string token, int index, double remainingWidth, out TextPart part)
        {
            for (; index < token.Length; index++)
            {
                // Skip any whitespace at beginning
                if (!char.IsWhiteSpace(token[index]))
                    break;
            }

            int startIndex = index;

            double width = 0;

            double lastValidWidth = 0;
            int lastValidEndIndex = index;

            for (; index < token.Length; index++)
            {
                char c = token[index];

                if (IsSplittable(c))
                {
                    lastValidWidth = width;
                    lastValidEndIndex = index;
                }

                ushort glyphIndex = m_glyphTypeface.CharacterToGlyphMap[token[index]];
                var glyphWidth = m_glyphTypeface.AdvanceWidths[glyphIndex] * m_fontSize;

                if (remainingWidth < glyphWidth)
                {
                    // Rollback to last valid end index and call it a night
                    Debug.Assert(lastValidEndIndex != startIndex); // Couldn't even place a single part?
                    width = lastValidWidth;
                    index = lastValidEndIndex;
                    break;
                }

                width += glyphWidth;
                remainingWidth -= glyphWidth;
            }

            part = new TextPart { Token = token, StartIndex = startIndex, EndIndex = index, Width = width };
            return index > startIndex;
        }

        private static bool IsSplittable(char c)
        {
            return char.IsWhiteSpace(c);
        }

        private double AdvanceLine(out double remainingWidth)
        {
            remainingWidth = m_maxSize.Width;
            return m_glyphTypeface.Height * m_fontSize;
        }

        #endregion

        #region Misc

        private static bool EqualsWithEpsilon(double a, double b)
        {
            return Math.Abs(a - b) <= 1e-3;
        }

        #endregion

        #endregion

        #region Nested Types

        private struct TextPart
        {
            public object Token;

            public double LineAdvance;
            public double Width;

            public int StartIndex;
            public int EndIndex;
        }
 
	    #endregion
    }
}
