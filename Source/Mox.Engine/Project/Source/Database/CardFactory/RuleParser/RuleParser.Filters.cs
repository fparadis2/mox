using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Mox.Abilities;

namespace Mox.Database
{
    partial class RuleParser
    {
        private Filter ParseFilter(string text)
        {
            text = ToSingular(text);

            var context = new FilterParsingContext();
            if (!Filters.Parse(this, text, context))
                return null;

            return context.Filter;
        }

        private class FilterParsingContext
        {
            public Filter Filter;
        }

        private class FilterParsers : ParserList<FilterParsingContext>
        {
            public FilterParsers()
            {
                // No prefix

                AddParser("creature or player", m => PermanentFilter.CreatureOrPlayer);

                // Card from your hand with prefix

                AddParser(RegexArgs.WordRun + "card from your hand", m =>
                {
                    Filter filter = HandFilter.Any;

                    if (!MatchCardPrefix(RegexArgs.ParseWordRun(m), ref filter))
                        return null;

                    return filter;
                });

                // Permanent with prefix

                AddParser(RegexArgs.WordRun + "creature you control", m =>
                {
                    Filter filter = PermanentFilter.AnyCreature & PermanentFilter.ControlledByYou;

                    if (!MatchCreaturePrefix(RegexArgs.ParseWordRun(m), ref filter))
                        return null;

                    return filter;
                });

                AddParser(RegexArgs.WordRun + "creature", m =>
                {
                    Filter filter = PermanentFilter.AnyCreature;

                    if (!MatchCreaturePrefix(RegexArgs.ParseWordRun(m), ref filter))
                        return null;

                    return filter;
                });

                // Player with prefix

                AddParser(RegexArgs.WordRun + "player", m =>
                {
                    var prefix = RegexArgs.ParseWordRun(m);
                    if (string.IsNullOrEmpty(prefix))
                        return PlayerFilter.Any;

                    return null;
                });

                // Spell with prefix

                AddParser(RegexArgs.WordRun + "spell", m =>
                {
                    Filter filter = StackFilter.AnySpell;

                    string prefix = RegexArgs.ParseWordRun(m);
                    if (!string.IsNullOrEmpty(prefix))
                        return null;

                    /*if (!MatchCreaturePrefix(RegexArgs.ParseWordRun(m), ref filter))
                        return null;*/

                    return filter;
                });
            }

            private static bool MatchCardPrefix(string prefix, ref Filter filter)
            {
                if (!string.IsNullOrEmpty(prefix))
                {
                    string[] prefixes = prefix.Split(' ');

                    foreach (var token in prefixes)
                    {
                        if (!MatchColorPrefix(token, ref filter) &&
                            !MatchTypePrefix(token, ref filter) &&
                            !MatchSuperTypePrefix(token, ref filter) &&
                            !MatchSubTypePrefix(token, ref filter))
                            return false;
                    }
                }

                return true;
            }

            private static bool MatchCreaturePrefix(string prefix, ref Filter filter)
            {
                if (!string.IsNullOrEmpty(prefix))
                {
                    string[] prefixes = prefix.Split(' ');

                    foreach (var token in prefixes)
                    {
                        if (!MatchColorPrefix(token, ref filter) &&
                            !MatchTypePrefix(token, ref filter) &&
                            !MatchSuperTypePrefix(token, ref filter) &&
                            !MatchSubTypePrefix(token, ref filter))
                            return false;
                    }
                }

                return true;
            }

            private static readonly Color[] ms_colors = new[] { Color.White, Color.Blue, Color.Black, Color.Red, Color.Green };
            private static bool MatchColorPrefix(string prefix, ref Filter filter)
            {
                foreach (var color in ms_colors)
                {
                    if (string.Equals(prefix, color.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        filter = filter & CardFilter.WithColor(color);
                        return true;
                    }
                }

                foreach (var color in ms_colors)
                {
                    if (string.Equals(prefix, "non" + color.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        filter = filter & CardFilter.NotWithColor(color);
                        return true;
                    }
                }

                return false;
            }

            private static readonly Type[] ms_types = new[] 
            {
                Type.Artifact,
                Type.Creature,
                Type.Enchantment,
                Type.Instant,
                Type.Land,
                Type.Planeswalker,
                Type.Sorcery,
                Type.Tribal,
                Type.Scheme
            };
            private static bool MatchTypePrefix(string prefix, ref Filter filter)
            {
                foreach (var type in ms_types)
                {
                    if (string.Equals(prefix, type.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        filter = filter & CardFilter.WithType(type);
                        return true;
                    }
                }

                foreach (var type in ms_types)
                {
                    if (string.Equals(prefix, "non" + type.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        filter = filter & CardFilter.NotWithType(type);
                        return true;
                    }
                }

                return false;
            }

            private static readonly SuperType[] ms_supertypes = new[] { SuperType.Basic, SuperType.Legendary, SuperType.World, SuperType.Snow };
            private static bool MatchSuperTypePrefix(string prefix, ref Filter filter)
            {
                foreach (var supertype in ms_supertypes)
                {
                    if (string.Equals(prefix, supertype.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        filter = filter & CardFilter.WithSuperType(supertype);
                        return true;
                    }
                }

                foreach (var supertype in ms_supertypes)
                {
                    if (string.Equals(prefix, "non" + supertype.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        filter = filter & CardFilter.NotWithSuperType(supertype);
                        return true;
                    }
                }

                return false;
            }

            private static bool MatchSubTypePrefix(string prefix, ref Filter filter)
            {
                foreach (SubType subtype in Enum.GetValues(typeof(SubType)))
                {
                    if (string.Equals(prefix, subtype.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        filter = filter & CardFilter.WithSubType(subtype);
                        return true;
                    }

                    if (string.Equals(prefix, "non-" + subtype.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        filter = filter & CardFilter.NotWithSubType(subtype);
                        return true;
                    }
                }

                return false;
            }

            private delegate Filter FilterDelegate(Match m);

            private void AddParser(string regex, FilterDelegate f)
            {
                AddParser(regex, (r, c, m) =>
                {
                    c.Filter = f(m);
                    return c.Filter != null;
                });
            }
        }

        private static readonly FilterParsers Filters = new FilterParsers();
    }
}
