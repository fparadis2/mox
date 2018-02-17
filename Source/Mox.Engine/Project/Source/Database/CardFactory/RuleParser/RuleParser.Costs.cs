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
                AddParser(@"\{T\}", (r, s, m) => new TapSelfCost(true));
                AddParser(RegexArgs.ManaCost, (r, s, m) =>
                {
                    if (RegexArgs.ParseManaCost(m, out ManaCost cost))
                        return new PayManaCost(cost);

                    return null;
                });

                AddParser("Sacrifice " + RegexArgs.TargetPermanents, (r, s, m) =>
                {
                    var cards = RegexArgs.ParseTargetPermanents(r, s, m, TargetContextType.SacrificeCost);
                    if (cards != null)
                        s.AddCost(new SacrificeCost(cards));
                    return true;
                });

                AddParser("Discard( all the cards in)? your hand", (r, s, m) =>
                {
                    return new DiscardHandCost();
                });

                AddParser("Discard " + RegexArgs.GetSimpleAmount() + " " + RegexArgs.Any + "(?<random> at random)?", (r, s, m) =>
                {
                    if (!RegexArgs.ParseAmount(r, m, out AmountResolver count))
                        return null;

                    var filterAny = RegexArgs.ParseAny(m) + " from your hand";
                    var filter = r.ParseFilter(filterAny);
                    if (filter == null)
                    {
                        r.AddUnknownFragment($"Filter (Discard)", filterAny);
                        return null;
                    }

                    bool atRandom = m.Groups["random"].Success;
                    return new DiscardCost(count, filter, atRandom);
                });
            }

            private delegate Cost CostCreator(RuleParser r, SpellDefinition s, Match m);

            private void AddParser(string regex, CostCreator creator)
            {
                AddParser(regex, (r, s, m) =>
                {
                    var cost = creator(r, s, m);
                    if (cost != null)
                    {
                        s.AddCost(cost);
                        return true;
                    }

                    return false;
                });
            }
        }

        private static readonly CostParsers CostParser = new CostParsers();
    }
}
