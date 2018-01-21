using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Mox.Abilities;

namespace Mox.Database
{
    partial class RuleParser
    {
        private static readonly Regex PeriodRegex = new Regex("(?!\\s|\\.|$)[^\\.\"]*((\"[^\"]*\")[^\\.\"]*)*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private bool ParseEffects(string effects, SpellDefinition spellDefinition, bool logUnknownFragments)
        {
            bool valid = true;

            foreach (var effect in MatchAndTrim(effects, PeriodRegex))
            {
                if (!EffectParser.Parse(this, effect, spellDefinition))
                {
                    if (logUnknownFragments)
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

                AddParser(RegexArgs.SelfName + " deals " + RegexArgs.SimpleAmount + " damage to " + RegexArgs.Targets, (r, s, m) =>
                {
                    if (!RegexArgs.ParseAmount(r, m, out AmountResolver damage))
                        return;

                    if (!RegexArgs.ParseTargets(r, m, out ObjectResolver targets))
                        return;

                    s.AddAction(new DealDamageAction(targets, damage));
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
