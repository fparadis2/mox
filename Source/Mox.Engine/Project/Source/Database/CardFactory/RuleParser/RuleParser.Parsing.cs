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

        #endregion

        #region Regex

        private static class RegexArgs
        {
            #region General

            public static readonly string SelfName = "~";

            #endregion

            #region Amount

            public static string GetSimpleAmount(int index = 0)
            {
                return $"(?<amount{index}>(a|an|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|thirteen|fourteen|fifteen|twenty|[0-9]+))";
            }

            public static bool ParseAmount(RuleParser ruleParser, Match match, out AmountResolver amount)
            {
                return ParseAmount(0, ruleParser, match, out amount);
            }

            public static bool ParseAmount(int index, RuleParser ruleParser, Match match, out AmountResolver amount)
            {
                string text = match.Groups["amount" + index].Value;

                if (AmountParser.Parse(text, out amount))
                    return true;

                ruleParser.AddUnknownFragment("SimpleAmount", text);
                amount = null;
                return false;
            }

            #endregion

            #region Mana

            public static readonly string ManaAmount = @"(?<mana>.+?)";
            public static string ParseManaAmount(Match match)
            {
                return match.Groups["mana"].Value;
            }

            private static readonly Regex ManaAmountSplitRegex = new Regex(" or |, or |, ", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            public static bool ParseManaAmounts(RuleParser ruleParser, Match match, out ManaAmount[] amounts)
            {
                string text = ParseManaAmount(match);

                if (text.Equals("one mana of any color"))
                {
                    amounts = new[]
                    {
                        new ManaAmount { White = 1 },
                        new ManaAmount { Blue = 1 },
                        new ManaAmount { Black = 1 },
                        new ManaAmount { Red = 1 },
                        new ManaAmount { Green = 1 },
                    };
                    return true;
                }

                List<ManaAmount> amountsList = new List<ManaAmount>();
                foreach (var token in SplitAndTrim(text, ManaAmountSplitRegex))
                {
                    ManaAmount amount;
                    if (!Mox.ManaAmount.TryParse(token, out amount))
                    {
                        ruleParser.AddUnknownFragment("Mana", text);
                        amounts = null;
                        return false;
                    }
                    amountsList.Add(amount);
                }

                amounts = amountsList.ToArray();
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

            public const string ManaCost = @"(?<manacost>(\{[A-Z\d/]+\})+)";
            public static bool ParseManaCost(Match match, out ManaCost cost)
            {
                return Mox.ManaCost.TryParse(match.Groups["manacost"].Value, ManaSymbolNotation.Long, out cost);
            }

            #endregion

            #region Power/Toughness

            public const string PT = "(?<pt>[+-][X0-9]+/[+-][X0-9]+)";
            public static bool ParsePT(RuleParser ruleParser, Match match, out AmountResolver power, out AmountResolver toughness)
            {
                power = toughness = null;

                string pt = match.Groups["pt"].Value;

                int separatorIndex = pt.IndexOf('/');
                if (separatorIndex < 0)
                {
                    ruleParser.AddUnknownFragment("PT", pt);
                    return false;
                }

                string powerString = pt.Substring(0, separatorIndex);
                string toughnessString = pt.Substring(separatorIndex + 1);

                if (!AmountParser.Parse(powerString, out power))
                {
                    ruleParser.AddUnknownFragment("Power", powerString);
                    return false;
                }

                if (!AmountParser.Parse(toughnessString, out toughness))
                {
                    ruleParser.AddUnknownFragment("Toughness", toughnessString);
                    return false;
                }

                return true;
            }

            #endregion

            #region Targets

            public const string Self = "(?<self>~)";
            public const string TargetChoice = "(a|an|target) (?<targets_choice>[^\\.]+)";
            public const string EachTarget = "(all|each) (?<targets_each>[^\\.]+)";

            public const string TargetsAny = "(" + Self + "|(?<targets_controller>you)|" + TargetChoice + "|" + EachTarget + ")";
            public static ObjectResolver ParseAnyTargets(RuleParser ruleParser, SpellDefinition spell, Match match, TargetContextType type)
            {
                var selfGroup = match.Groups["self"];
                if (selfGroup.Success)
                    return ObjectResolver.SpellSource;

                var controllerGroup = match.Groups["targets_controller"];
                if (controllerGroup.Success)
                    return ObjectResolver.SpellController;

                if (MatchTargets_Target(ruleParser, spell, match, type, FilterType.All, out ObjectResolver targetResult))
                    return targetResult;

                if (MatchTargets_Each(ruleParser, spell, match, FilterType.All, out ObjectResolver eachResult))
                    return eachResult;

                throw new InvalidProgramException("Did not match the regex?");
            }

            public const string TargetPermanents = "(" + Self + "|" + TargetChoice + "|" + EachTarget + ")";
            public static ObjectResolver ParseTargetPermanents(RuleParser ruleParser, SpellDefinition spell, Match match, TargetContextType type)
            {
                var selfGroup = match.Groups["self"];
                if (selfGroup.Success)
                    return ObjectResolver.SpellSource;

                if (MatchTargets_Target(ruleParser, spell, match, type, FilterType.Permanent, out ObjectResolver targetResult))
                    return targetResult;

                if (MatchTargets_Each(ruleParser, spell, match, FilterType.Permanent, out ObjectResolver eachResult))
                    return eachResult;

                throw new InvalidProgramException("Did not match the regex?");
            }

            public const string TargetPlayers = "((?<targets_controller>you)|" + TargetChoice + "|" + EachTarget + ")";
            public static ObjectResolver ParseTargetPlayers(RuleParser ruleParser, SpellDefinition spell, Match match, TargetContextType type)
            {
                var controllerGroup = match.Groups["targets_controller"];
                if (controllerGroup.Success)
                    return ObjectResolver.SpellController;

                if (MatchTargets_Target(ruleParser, spell, match, type, FilterType.Player, out ObjectResolver targetResult))
                    return targetResult;

                if (MatchTargets_Each(ruleParser, spell, match, FilterType.Player, out ObjectResolver eachResult))
                    return eachResult;

                throw new InvalidProgramException("Did not match the regex?");
            }

            private static bool MatchTargets_Target(RuleParser ruleParser, SpellDefinition spell, Match match, TargetContextType type, FilterType expectedType, out ObjectResolver result)
            {
                var targetGroup = match.Groups["targets_choice"];
                if (targetGroup.Success)
                {
                    var filter = ruleParser.ParseFilter(targetGroup.Value);
                    if (filter != null)
                    {
                        Debug.Assert(expectedType.HasFlag(filter.FilterType));

                        switch (type)
                        {
                            case TargetContextType.SacrificeCost:
                                Debug.Assert(filter.FilterType.HasFlag(FilterType.Permanent));
                                filter = filter & PermanentFilter.ControlledByYou; // Implicit
                                break;
                            default:
                                break;
                        }

                        var cost = new TargetCost(type, filter);
                        spell.AddCost(cost);
                        result = new TargetObjectResolver(cost);
                    }
                    else
                    {
                        ruleParser.AddUnknownFragment($"Target ({expectedType})", targetGroup.Value);
                        result = null;
                    }
                    return true;
                }

                result = null;
                return false;
            }

            private static bool MatchTargets_Each(RuleParser ruleParser, SpellDefinition spell, Match match, FilterType expectedType, out ObjectResolver result)
            {
                var eachGroup = match.Groups["targets_each"];
                if (eachGroup.Success)
                {
                    var filter = ruleParser.ParseFilter(eachGroup.Value);
                    if (filter != null)
                    {
                        Debug.Assert(expectedType.HasFlag(filter.FilterType));
                        result = new FilterObjectResolver(filter);
                    }
                    else
                    {
                        ruleParser.AddUnknownFragment($"Each ({expectedType})", eachGroup.Value);
                        result = null;
                    }
                    return true;
                }

                result = null;
                return false;
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
