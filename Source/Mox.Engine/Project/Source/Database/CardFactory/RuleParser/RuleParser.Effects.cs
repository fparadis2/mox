﻿using System;
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
                        return true; // Logs its own unknown fragment

                    s.AddAction(new GainManaAction(color));
                    return true;
                });

                AddParser(RegexArgs.SelfName + " deals " + RegexArgs.SimpleAmount + " damage to " + RegexArgs.Targets, (r, s, m) =>
                {
                    if (!RegexArgs.ParseAmount(r, m, out AmountResolver damage))
                        return true; // Logs its own unknown fragment

                    var targets = RegexArgs.ParseTargets(r, s, m);
                    if (targets == null)
                        return true; // Logs its own unknown fragment

                    s.AddAction(new DealDamageAction(targets, damage));
                    return true;
                });
            }

            private delegate Mox.Abilities.Action ActionCreator(RuleParser r, Match m);

            private void AddParser(string regex, ActionCreator creator)
            {
                AddParser(regex, (r, s, m) =>
                {
                    var action = creator(r, m);
                    if (action != null)
                    {
                        s.AddAction(action);
                    }
                    return true;
                });
            }
        }

        private static readonly EffectParsers EffectParser = new EffectParsers();
    }
}
