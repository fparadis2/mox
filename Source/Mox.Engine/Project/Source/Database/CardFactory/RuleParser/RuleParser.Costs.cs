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
            bool valid = true;

            foreach (var cost in SplitAndTrim(costs, new[] { ',' }))
            {
                if (!CostParser.Parse(this, cost, spellDefinition))
                {
                    AddUnknownFragment("Cost", cost);
                    valid = false;
                }
            }

            return valid;
        }

        private class CostParsers : ParserList<SpellDefinition>
        {
            public CostParsers()
            {
                AddParser(@"\{T\}", m => Costs.TapSelf());
                AddParser(RegexArgs.ManaCost, m =>
                {
                    if (RegexArgs.ParseManaCost(m, out ManaCost cost))
                        return new PayManaCost(cost);

                    return null;
                });
            }

            private delegate Cost CostCreator(Match m);

            private void AddParser(string regex, CostCreator creator)
            {
                AddParser(regex, (r, s, m) =>
                {
                    var cost = creator(m);
                    if (cost != null)
                    {
                        s.AddCost(cost);
                    }
                });
            }
        }

        private static readonly CostParsers CostParser = new CostParsers();
    }
}
