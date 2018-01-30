using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private static bool TryParseNumber(string number, out int value)
        {
            value = 0;

            if (string.IsNullOrEmpty(number))
                return false;

            if (int.TryParse(number, out value))
                return true;

            switch (number)
            {
                case "no": value = 0; return true;
                case "a": value = 1; return true;
                case "an": value = 1; return true;
                case "one": value = 1; return true;
                case "two": value = 2; return true;
                case "three": value = 3; return true;
                case "four": value = 4; return true;
                case "five": value = 5; return true;
                case "six": value = 6; return true;
                case "seven": value = 7; return true;
                case "eight": value = 8; return true;
                case "nine": value = 9; return true;
                case "ten": value = 10; return true;
                case "eleven": value = 11; return true;
                case "twelve": value = 12; return true;
                case "thirteen": value = 13; return true;
                case "fourteen": value = 14; return true;
                case "fifteen": value = 15; return true;
                case "twenty": value = 20; return true;
                case "ninety-nine": value = 99; return true;
                default: return false;
            }
        }

        #endregion

        #region Regex

        private static class RegexArgs
        {
            #region General

            public static readonly string SelfName = "~";

            #endregion

            #region Amount

            public static readonly string SimpleAmount = "(?<amount>(a|an|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|thirteen|fourteen|fifteen|twenty|[0-9]+))";
            public static bool ParseAmount(RuleParser ruleParser, Match match, out AmountResolver amount)
            {
                string text = match.Groups["amount"].Value;

                if (TryParseNumber(text, out int value))
                {
                    amount = new ConstantAmountResolver(value);
                    return true;
                }

                ruleParser.AddUnknownFragment("SimpleAmount", text);
                amount = null;
                return false;
            }

            #endregion

            #region Mana

            public static readonly string Mana = @"(?<mana>.+?)";
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

            #region ManaCost

            public static readonly string ManaCost = @"(?<manacost>(\{[A-Z\d/]+\})+)";
            public static bool ParseManaCost(Match match, out ManaCost cost)
            {
                return Mox.ManaCost.TryParse(match.Groups["manacost"].Value, ManaSymbolNotation.Long, out cost);
            }

            #endregion

            #region Targets

            public const string TargetChoice = "target (?<targets_choice>[^\\.]+)";

            public const string TargetsAny = "((?<targets_controller>you)|" + TargetChoice + ")";
            public static ObjectResolver ParseAnyTargets(RuleParser ruleParser, SpellDefinition spell, Match match)
            {
                var controllerGroup = match.Groups["targets_controller"];
                if (controllerGroup.Success)
                    return ObjectResolver.SpellController;

                var targetGroup = match.Groups["targets_choice"];
                if (targetGroup.Success)
                {
                    var filter = ruleParser.ParseFilter(targetGroup.Value);
                    if (filter != null)
                    {
                        var cost = new TargetCost(filter);
                        spell.AddCost(cost);
                        return new TargetObjectResolver(cost);
                    }

                    ruleParser.AddUnknownFragment("Targets (Any)", targetGroup.Value);
                    return null;
                }

                throw new InvalidProgramException("Did not match the regex?");
            }

            public const string TargetPermanents = "(" + TargetChoice + ")";
            public static ObjectResolver ParseTargetPermanents(RuleParser ruleParser, SpellDefinition spell, Match match)
            {
                var targetGroup = match.Groups["targets_choice"];
                if (targetGroup.Success)
                {
                    var filter = ruleParser.ParseFilter(targetGroup.Value);
                    if (filter != null)
                    {
                        Debug.Assert(filter.FilterType.HasFlag(FilterType.Permanent));

                        var cost = new TargetCost(filter);
                        spell.AddCost(cost);
                        return new TargetObjectResolver(cost);
                    }

                    ruleParser.AddUnknownFragment("Targets (Permanent)", targetGroup.Value);
                    return null;
                }

                throw new InvalidProgramException("Did not match the regex?");
            }

            #endregion

            #region WordRun

            public const string WordRun = "(?<wordrun>[^\\.\"]?)";
            public static string ParseWordRun(Match match)
            {
                return match.Groups["wordrun"].Value.Trim();
            }

            #endregion
        }

        #endregion

        #region ParserList

        private class ParserList<T>
        {
            protected delegate bool ParserDelegate(RuleParser ruleParser, T context, Match match);

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
                        if (parser.Functor(ruleParser, context, match))
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
