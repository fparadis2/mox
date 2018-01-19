using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Mox.Database
{
    partial class RuleParser
    {
        #region Constants

        private static readonly char[] RuleSeparators = new[] { '\n' };
        private static readonly char[] AbilitySeparators = new[] { ',' };

        private static readonly Regex ReminderTextRegex = new Regex(@"\(.*?\)", RegexOptions.Compiled);

        #endregion

        #region Methods

        private static IEnumerable<string> SplitAndTrim(string text, char[] separators)
        {
            var tokens = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                string trimmed = token.Trim();
                if (trimmed.Length == 0)
                    continue;

                yield return trimmed;
            }
        }

        private static string RemoveReminderText(string text)
        {
            return ReminderTextRegex.Replace(text, string.Empty);
        }

        private static string RemoveThisName(CardInfo cardInfo, string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return text.Replace(cardInfo.Name, "~");
        }

        #endregion

        #region Regex

        private static class RegexArgs
        {
            #region Mana

            public static string Mana = @"(?<mana>.+)";
            public static string ParseMana(Match match)
            {
                return match.Groups["mana"].Value;
            }

            public static bool ParseManaColors(Match match, out Color color)
            {
                string text = ParseMana(match);

                return ParseSingleColor(text, out color);
            }

            private static bool ParseSingleColor(string text, out Color color)
            {
                switch (text)
                {
                    case "{C}":
                        color = Color.None;
                        return true;

                    case "{W}":
                        color = Color.White;
                        return true;

                    case "{U}":
                        color = Color.Blue;
                        return true;

                    case "{B}":
                        color = Color.Black;
                        return true;

                    case "{R}":
                        color = Color.Red;
                        return true;

                    case "{G}":
                        color = Color.Green;
                        return true;

                    default:
                        color = Color.None;
                        return false;
                }
            }

            #endregion
        }

        #endregion
    }
}
