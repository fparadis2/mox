using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Mox.Abilities;

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

        private static IEnumerable<string> MatchAndTrim(string text, Regex regex)
        {
            var matches = regex.Matches(text);

            foreach (Match match in matches)
            {
                yield return match.Value.Trim();
            }
        }

        private static IEnumerable<string> SplitAndTrim(string text, Regex regex)
        {
            var tokens = regex.Split(text);
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

            public static string Mana = @"(?<mana>.+?)";
            public static string ParseMana(Match match)
            {
                return match.Groups["mana"].Value;
            }

            private static readonly Regex ManaSplitRegex = new Regex(" or |, or |, ", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            public static bool ParseManaColors(RuleParser ruleParser, Match match, out Color color)
            {
                string text = ParseMana(match);

                if (text.Equals("one mana of any color"))
                {
                    color = ColorExtensions.AllColors;
                    return true;
                }

                color = Color.None;
                foreach (var token in SplitAndTrim(text, ManaSplitRegex))
                {
                    if (ParseSingleColor(token, out Color singleColor))
                    {
                        color |= singleColor;
                    }
                    else
                    {
                        ruleParser.AddUnknownFragment("Mana", text);
                        return false;
                    }
                }

                return true;
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

        #region ParserList

        private class ParserList<T>
        {
            protected delegate void ParserDelegate(RuleParser ruleParser, T context, Match match);

            private readonly List<Parser> m_parsers = new List<Parser>();

            private struct Parser
            {
                public Regex Regex;
                public ParserDelegate Functor;
            }

            public bool Parse(RuleParser ruleParser, string text, T context)
            {
                foreach (var parser in m_parsers)
                {
                    var match = parser.Regex.Match(text);
                    if (match.Success)
                    {
                        parser.Functor(ruleParser, context, match);
                        return true;
                    }
                }

                return false;
            }

            protected void AddParser(string regex, ParserDelegate parserFunctor)
            {
                Regex r = new Regex("^(" + regex + ")$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var parser = new Parser { Regex = r, Functor = parserFunctor };
                m_parsers.Add(parser);
            }
        }

        #endregion
    }
}
