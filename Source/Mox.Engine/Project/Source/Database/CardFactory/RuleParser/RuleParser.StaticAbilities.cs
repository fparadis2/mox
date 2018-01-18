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
        private class StaticAbilityParsers
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

            private readonly List<Parser> m_parsers = new List<Parser>();

            private struct Parser
            {
                public Regex Regex;
                public IAbilityCreator Creator;
            }

            public bool TryGetCreator(string text, out IAbilityCreator creator)
            {
                foreach (var parser in m_parsers)
                {
                    if (parser.Regex.IsMatch(text))
                    {
                        creator = parser.Creator;
                        return true;
                    }
                }

                creator = null;
                return false;
            }

            private void Add<TAbility>(string regex)
                where TAbility : Ability, new()
            {
                AddParser(regex, new AbilityCreator<TAbility>());
            }

            private void Ignore(string regex)
            {
                AddParser(regex, null);
            }

            private void AddParser(string regex, IAbilityCreator creator)
            {
                Regex r = new Regex("^(" + regex + ")$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var parser = new Parser { Regex = r, Creator = creator };
                m_parsers.Add(parser);
            }
        }

        private static readonly StaticAbilityParsers StaticAbility = new StaticAbilityParsers();
    }
}
