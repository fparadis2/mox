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
        private bool ParseCosts(string costs, SpellDefinition spellDefinition)
        {
            foreach (var cost in SplitAndTrim(costs, new[] { ',' }))
            {
                if (!CostParser.Parse(cost, spellDefinition))
                    return false;
            }

            return true;
        }

        private class CostParsers
        {
            public CostParsers()
            {
#warning todo spell_v2 tests
                AddParser(@"\{T\}", m => Costs.TapSelf());
            }

            private readonly List<CostParser> m_parsers = new List<CostParser>();

            private struct CostParser
            {
                public Regex Regex;
                public SpellCreator Creator;
            }

            public bool Parse(string text, SpellDefinition spell)
            {
                foreach (var parser in m_parsers)
                {
                    var match = parser.Regex.Match(text);
                    if (match.Success)
                    {
#warning todo log match with false result
                        return parser.Creator(spell, match);
                    }
                }

                return false;
            }

            private delegate Cost CostCreator(Match m);

            private void AddParser(string regex, CostCreator creator)
            {
                AddParser(regex, (s, m) =>
                {
                    var cost = creator(m);
                    if (cost != null)
                    {
                        s.AddCost(cost);
                        return true;
                    }
                    return false;
                });
            }

            private void AddParser(string regex, SpellCreator creator)
            {
                Regex r = new Regex("^(" + regex + ")$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var parser = new CostParser { Regex = r, Creator = creator };
                m_parsers.Add(parser);
            }
        }

        private static readonly CostParsers CostParser = new CostParsers();
    }
}
