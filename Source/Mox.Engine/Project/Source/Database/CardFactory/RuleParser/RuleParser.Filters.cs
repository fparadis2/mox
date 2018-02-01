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
        private Filter ParseFilter(string text, TargetContextType type)
        {
            var context = new FilterParsingContext();
            if (!Filters.Parse(this, text, context))
                return null;

            var filter = context.Filter;

            switch (type)
            {
                case TargetContextType.SacrificeCost:
                    filter = filter & PermanentFilter.ControlledByYou; // Implicit
                    break;
                default:
                    break;
            }

            return filter;
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

                // With prefix

                AddParser(RegexArgs.WordRun + "creature you control", m =>
                {
                    var prefix = RegexArgs.ParseWordRun(m);
                    if (string.IsNullOrEmpty(prefix))
                        return PermanentFilter.AnyCreature & PermanentFilter.ControlledByYou;

                    return null;
                });

                AddParser(RegexArgs.WordRun + "creature", m =>
                {
                    var prefix = RegexArgs.ParseWordRun(m);
                    if (string.IsNullOrEmpty(prefix))
                        return PermanentFilter.AnyCreature;

                    return null;
                });

                AddParser(RegexArgs.WordRun + "player", m =>
                {
                    var prefix = RegexArgs.ParseWordRun(m);
                    if (string.IsNullOrEmpty(prefix))
                        return PlayerFilter.Any;

                    return null;
                });
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
