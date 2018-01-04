using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
                public Initializer Initializer;
            }

            public Initializer GetInitializer(string text)
            {
                foreach (var parser in m_parsers)
                {
                    if (parser.Regex.IsMatch(text))
                        return parser.Initializer;
                }

                return null;
            }

            private void Add<TAbility>(string regex)
                where TAbility : Ability, new()
            {
                AddParser(regex, card =>
                {
                    AddAbility<TAbility>(card);
                });
            }

            private void Ignore(string regex)
            {
                AddParser(regex, c => { });
            }

            private void AddParser(string regex, Initializer initializer)
            {
                Regex r = new Regex("^(" + regex + ")$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var parser = new Parser { Regex = r, Initializer = initializer };
                m_parsers.Add(parser);
            }
        }

        private static readonly StaticAbilityParsers StaticAbility = new StaticAbilityParsers();
    }
}
