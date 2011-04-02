using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mox.UI
{
    public static class SymbolTextTokenizer
    {
        #region Methods

        /// <summary>
        /// Tokenizes a string into symbols.
        /// </summary>
        /// <remarks>
        /// Can return strings, ints for colorless mana and ManaSymbols
        /// </remarks>
        /// <returns></returns>
        public static IEnumerable<object> Tokenize(string text, ManaSymbolNotation notation)
        {
            if (string.IsNullOrEmpty(text))
            {
                yield break;
            }

            int wordStart = 0;
            int end = 0;

            while (end != -1)
            {
                int currentStart = end;
                string word = GetNextWord(text, ref currentStart, out end);

                IEnumerable<object> result;
                if (TryHandleWord(word, notation, out result))
                {
                    if (currentStart != wordStart)
                    {
                        yield return text.Substring(wordStart, currentStart - wordStart);
                    }

                    wordStart = end;

                    foreach (object o in result)
                    {
                        yield return o;
                    }
                }
            }

            if (end != wordStart)
            {
                yield return text.Substring(wordStart);
            }
        }

        private static bool TryHandleWord(string word, ManaSymbolNotation notation, out IEnumerable<object> result)
        {
            if (!string.IsNullOrEmpty(word))
            {
                ManaCost cost;
                if (ManaCost.TryParse(word, notation, out cost))
                {
                    result = cost.ToObjects();
                    return true;
                }

                MiscSymbols miscSymbol;
                if (MiscSymbols.TryParse(word, out miscSymbol))
                {
                    result = new[] { miscSymbol };
                    return true;
                }
            }

            result = null;
            return false;
        }

        private static string GetNextWord(this string text, ref int start, out int end)
        {
            Debug.Assert(start < text.Length);

            start = AdvanceToNonSeparator(text, start);
            end = text.IndexOfSeparator(start);

            if (end == -1)
            {
                return text.Substring(start);
            }

            return text.Substring(start, end - start);
        }

        private static readonly char[] Separators = new[] { ' ', '\t', '\n', '\r', ',', '.', ':', '(', ')', '[', ']' };

        private static int AdvanceToNonSeparator(this string text, int index)
        {
            while (index < text.Length && IsSeparator(text[index]))
            {
                index++;
            }

            return index;
        }

        private static int IndexOfSeparator(this string text, int startIndex)
        {
            return text.IndexOfAny(Separators, startIndex);
        }

        private static bool IsSeparator(char character)
        {
            return Separators.Contains(character);
        }

        #endregion
    }
}
