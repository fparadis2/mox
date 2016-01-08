using System.Collections.Generic;
using NUnit.Framework;

namespace Mox.UI
{
    [TestFixture]
    public class TextTokenizerTests
    {
        #region Setup / Utils

        private List<TextTokenizer.Token> Tokenize(string text)
        {
            List<TextTokenizer.Token> tokens = new List<TextTokenizer.Token>();
            TextTokenizer.Tokenize(text, tokens);
            return tokens;
        }

        private static void CheckToken(TextTokenizer.Token token, TextTokenizer.TokenType type, int startIndex, int endIndex)
        {
            Assert.AreEqual(type, token.Type);
            Assert.AreEqual(startIndex, token.StartIndex);
            Assert.AreEqual(endIndex, token.EndIndex);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Tokenize_empty_string()
        {
            Assert.Collections.IsEmpty(Tokenize(null));
            Assert.Collections.IsEmpty(Tokenize(string.Empty));
        }

        [Test]
        public void Test_Tokenize_single_word()
        {
            var tokens = Tokenize("Hello");
            Assert.AreEqual(1, tokens.Count);
            CheckToken(tokens[0], TextTokenizer.TokenType.Text, 0, 5);
        }

        [Test]
        public void Test_Tokenize_space()
        {
            var tokens = Tokenize(" ");
            Assert.AreEqual(1, tokens.Count);
            CheckToken(tokens[0], TextTokenizer.TokenType.Whitespace, 0, 1);
        }

        [Test]
        public void Test_Tokenize_tabs()
        {
            var tokens = Tokenize("\t");
            Assert.AreEqual(1, tokens.Count);
            CheckToken(tokens[0], TextTokenizer.TokenType.Whitespace, 0, 1);
        }

        [Test]
        public void Test_Tokenize_whitespace()
        {
            var tokens = Tokenize(" \t ");
            Assert.AreEqual(1, tokens.Count);
            CheckToken(tokens[0], TextTokenizer.TokenType.Whitespace, 0, 3);
        }

        [Test]
        public void Test_Tokenize_newline()
        {
            var tokens = Tokenize("\n\n");
            Assert.AreEqual(2, tokens.Count);

            for (int i = 0; i < tokens.Count; i++)
            {
                CheckToken(tokens[i], TextTokenizer.TokenType.NewLine, i, i + 1);
            }
        }

        [Test]
        public void Test_Tokenize_fills_start_and_end_indices_correctly()
        {
            var tokens = Tokenize("Hello, my name    is \nEarl");
            Assert.AreEqual(11, tokens.Count);

            for (int i = 0; i < tokens.Count; i++)
            {
                if (i > 0)
                {
                    Assert.AreEqual(tokens[i - 1].EndIndex, tokens[i].StartIndex);
                }
                else
                {
                    Assert.AreEqual(0, tokens[i].StartIndex);
                }

                Assert.Greater(tokens[i].EndIndex, tokens[i].StartIndex);
            }
        }

        [Test]
        public void Test_Tokenize_splits_words_according_to_separators_and_whitepace()
        {
            var tokens = Tokenize("Hello, my name    is \nEarl");
            Assert.AreEqual(11, tokens.Count);

            CheckToken(tokens[0], TextTokenizer.TokenType.Text, 0, 5);
            CheckToken(tokens[1], TextTokenizer.TokenType.Separator, 5, 6);
            CheckToken(tokens[2], TextTokenizer.TokenType.Whitespace, 6, 7);
            CheckToken(tokens[3], TextTokenizer.TokenType.Text, 7, 9);
            CheckToken(tokens[4], TextTokenizer.TokenType.Whitespace, 9, 10);
            CheckToken(tokens[5], TextTokenizer.TokenType.Text, 10, 14);
            CheckToken(tokens[6], TextTokenizer.TokenType.Whitespace, 14, 18);
            CheckToken(tokens[7], TextTokenizer.TokenType.Text, 18, 20);
            CheckToken(tokens[8], TextTokenizer.TokenType.Whitespace, 20, 21);
            CheckToken(tokens[9], TextTokenizer.TokenType.NewLine, 21, 22);
            CheckToken(tokens[10], TextTokenizer.TokenType.Text, 22, 26);
        }

        #endregion
    }
}