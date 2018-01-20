using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Mox.Abilities;

namespace Mox.Database
{
    partial class RuleParser
    {
        private static readonly Regex PeriodRegex = new Regex("(?!\\s|\\.|$)[^\\.\"]*((\"[^\"]*\")[^\\.\"]*)*");

        private bool ParseEffects(string effects, SpellDefinition spellDefinition)
        {
            bool valid = true;

            foreach (var effect in SplitAndTrim(effects, PeriodRegex))
            {
                if (!EffectParser.Parse(this, effect, spellDefinition))
                {
                    AddUnknownFragment("Effect", effect);
                    valid = false;
                }
            }

            return valid;
        }

        private class EffectParsers : ParserList<SpellDefinition>
        {
            public EffectParsers()
            {
                AddParser(@"Add " + RegexArgs.Mana + @" to your mana pool", (r, s, m) =>
                {
                    if (!RegexArgs.ParseManaColors(r, m, out Color color))
                        return;

                    s.AddAction(new GainManaAction(color));
                });
            }

            private delegate Mox.Abilities.Action ActionCreator(Match m);

            private void AddParser(string regex, ActionCreator creator)
            {
                AddParser(regex, (r, s, m) =>
                {
                    var action = creator(m);
                    if (action != null)
                    {
                        s.AddAction(action);
                    }
                });
            }
        }

        private static readonly EffectParsers EffectParser = new EffectParsers();
    }
}
