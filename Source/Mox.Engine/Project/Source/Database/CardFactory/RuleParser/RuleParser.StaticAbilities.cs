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
        private bool ParseStaticAbilityList(string text)
        {
            List<string> unknownFragments = new List<string>();
            int count = 0;

            foreach (var ability in SplitAndTrim(text, AbilitySeparators))
            {
                count++;

                StaticAbilityContext context = new StaticAbilityContext();
                if (StaticAbilities.Parse(this, ability, context))
                {
                    if (context.Creator != null)
                        m_abilities.Add(context.Creator);
                }
                else
                {
                    unknownFragments.Add(ability);
                }
            }

            if (unknownFragments.Count > 0 && unknownFragments.Count < count)
            {
                foreach (var fragment in unknownFragments)
                {
                    AddUnknownFragment("Rule", fragment);
                }

                return true;
            }

            return unknownFragments.Count == 0;
        }

        private class StaticAbilityContext
        {
            public IAbilityCreator Creator;
        }

        private class StaticAbilityParsers : ParserList<StaticAbilityContext>
        {
            public StaticAbilityParsers()
            {
                Ignore("Devoid"); // Already taken into account in card color

                Add<DefenderAbility>("Defender");
                Add<DoubleStrikeAbility>("Double Strike");
                Add<FirstStrikeAbility>("First Strike");
                Add<FlashAbility>("Flash");
                Add<FlyingAbility>("Flying");
                Add<HasteAbility>("Haste");
                Add<ReachAbility>("Reach");
                Add<TrampleAbility>("Trample");
                Add<VigilanceAbility>("Vigilance");
            }

            private void Add<TAbility>(string regex)
                where TAbility : Ability, new()
            {
                AddParser(regex, (r, c, m) =>
                {
                    c.Creator = new AbilityCreator<TAbility>();
                    return true;
                });
            }

            private void Ignore(string regex)
            {
                AddParser(regex, (r, c, m) => { return true; });
            }
        }

        private static readonly StaticAbilityParsers StaticAbilities = new StaticAbilityParsers();
    }
}
