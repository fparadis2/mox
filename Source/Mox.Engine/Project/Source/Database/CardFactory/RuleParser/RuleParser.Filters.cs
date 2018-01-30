using System;
using System.Collections.Generic;
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

                AddParser("creature or player", (r, c, m) =>
                {
                    c.Filter = PermanentFilter.CreatureOrPlayer;
                    return true;
                });

                // With prefix

                AddParser(RegexArgs.WordRun + "creature", (r, c, m) =>
                {
                    var prefix = RegexArgs.ParseWordRun(m);
                    if (string.IsNullOrEmpty(prefix))
                    {
                        c.Filter = PermanentFilter.AnyCreature;
                        return true;
                    }

                    return false;
                });

                AddParser(RegexArgs.WordRun + "player", (r, c, m) =>
                {
                    var prefix = RegexArgs.ParseWordRun(m);
                    if (string.IsNullOrEmpty(prefix))
                    {
                        c.Filter = PlayerFilter.Any;
                        return true;
                    }

                    return false;
                });
            }
        }

        private static readonly FilterParsers Filters = new FilterParsers();
    }
}
