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
                Add<FlyingAbility>("Flying");
                Add<ReachAbility>("Reach");
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
                where TAbility : StaticAbility, new()
            {
                AddParser(regex, card =>
                {
                    card.Manager.CreateAbility<TAbility>(card);
                });
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
