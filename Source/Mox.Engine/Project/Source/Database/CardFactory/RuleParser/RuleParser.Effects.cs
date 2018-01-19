using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Mox.Abilities;

namespace Mox.Database
{
    partial class RuleParser
    {
        private bool ParseEffects(string effects, SpellDefinition spellDefinition)
        {
            foreach (var effect in SplitAndTrim(effects, new[] { '.' }))
            {
                if (!EffectParser.Parse(effect, spellDefinition))
                    return false;
            }

            return true;
        }

        private class EffectParsers
        {
            public EffectParsers()
            {
                AddParser(@"Add " + RegexArgs.Mana + @" to your mana pool", m =>
                {
                    if (!RegexArgs.ParseManaColors(m, out Color color))
                        return null;

                    return new GainManaAction(color);
                });
            }

            private readonly List<EffectParser> m_parsers = new List<EffectParser>();

            private struct EffectParser
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
#warning todo spell_v2 find way to log match with unknown result...
                        return parser.Creator(spell, match);
                    }
                }

                return false;
            }

            private delegate Mox.Abilities.Action ActionCreator(Match m);

            private void AddParser(string regex, ActionCreator creator)
            {
                AddParser(regex, (s, m) =>
                {
                    var action = creator(m);
                    if (action != null)
                    {
                        s.AddAction(action);
                        return true;
                    }

                    return false;
                });
            }

            private void AddParser(string regex, SpellCreator creator)
            {
                Regex r = new Regex("^(" + regex + ")$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var parser = new EffectParser { Regex = r, Creator = creator };
                m_parsers.Add(parser);
            }
        }

        private static readonly EffectParsers EffectParser = new EffectParsers();
    }
}
