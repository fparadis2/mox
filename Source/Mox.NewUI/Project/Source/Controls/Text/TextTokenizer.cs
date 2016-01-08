using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mox.UI
{
    public static class TextTokenizer
    {
        public static void Tokenize(string text, List<Token> tokens, int index = 0, int endIndex = -1)
        {
            if (string.IsNullOrEmpty(text))
                return;

            if (index < 0 || index >= text.Length)
                throw new ArgumentException("Invalid start index", "index");

            if (endIndex < 0)
                endIndex = text.Length;

            int lastTokenStart = index;
            TokenType lastType = GetTokenType(text[index++]);

            for (; index < endIndex; index++)
            {
                TokenType type = GetTokenType(text[index]);

                if (type != lastType || lastType == TokenType.NewLine)
                {
                    CreateToken(tokens, lastType, ref lastTokenStart, index);
                    lastType = type;
                }
            }

            // Last token
            Debug.Assert(index == endIndex);
            CreateToken(tokens, lastType, ref lastTokenStart, index);
        }

        private static TokenType GetTokenType(char c)
        {
            switch (c)
            {
                case ' ':
                case '\t':
                    return TokenType.Whitespace;

                case '\n':
                    return TokenType.NewLine;

                case ',':
                case '.':
                case ':':
                case '(':
                case ')':
                case '[':
                case ']':
                    return TokenType.Separator;

                default:
                    return TokenType.Text;
            }
        }

        private static void CreateToken(List<Token> tokens, TokenType type, ref int start, int end)
        {
            tokens.Add(new Token { Type = type, StartIndex = (short)start, EndIndex = (short)end });
            start = end;
        }

        public struct Token
        {
            public TokenType Type;
            public short StartIndex;
            public short EndIndex;
        }

        public enum TokenType : byte
        {
            Text,
            Separator,
            Whitespace,
            NewLine,
        }
    }
}