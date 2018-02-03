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
                // Direct

                AddParser(@"Add " + RegexArgs.ManaAmount + @" to your mana pool", (r, s, m) =>
                {
                    if (!RegexArgs.ParseManaAmounts(r, m, out ManaAmount[] amounts))
                        return null;

                    return new GainManaAction(amounts);
                });

                AddParser(@"Tap " + RegexArgs.TargetPermanents, (r, s, m) =>
                {
                    var targets = ParseTargetPermanents(r, s, m);
                    if (targets == null)
                        return null;

                    return new TapAction(targets);
                });

                // From card

                AddParser(RegexArgs.SelfName + " deals " + RegexArgs.GetSimpleAmount() + " damage to " + RegexArgs.TargetsAny, (r, s, m) =>
                {
                    if (!RegexArgs.ParseAmount(r, m, out AmountResolver damage))
                        return null;

                    var targets = ParseAnyTargets(r, s, m);
                    if (targets == null)
                        return null;

                    return new DealDamageAction(targets, damage);
                });

                // Players

                AddParser(RegexArgs.TargetPlayers + " lose(s)? " + RegexArgs.GetSimpleAmount() + " life", (r, s, m) =>
                {
                    if (!RegexArgs.ParseAmount(r, m, out AmountResolver life))
                        return null;

                    var targets = ParseTargetPlayers(r, s, m);
                    if (targets == null)
                        return null;

                    return new LoseLifeAction(targets, life);
                });

                AddParser(RegexArgs.TargetPlayers + " lose(s)? " + RegexArgs.GetSimpleAmount(0) + " life and you gain " + RegexArgs.GetSimpleAmount(1) + " life", (r, s, m) =>
                {
                    if (!RegexArgs.ParseAmount(0, r, m, out AmountResolver life))
                        return true;

                    if (!RegexArgs.ParseAmount(1, r, m, out AmountResolver yourLife))
                        return true;

                    var targets = ParseTargetPlayers(r, s, m);
                    if (targets == null)
                        return true;

                    s.AddAction(new LoseLifeAction(targets, life));
                    s.AddAction(new GainLifeAction(ObjectResolver.SpellController, yourLife));
                    return true;
                });

                AddParser(RegexArgs.TargetPlayers + " gain(s)? " + RegexArgs.GetSimpleAmount() + " life", (r, s, m) =>
                {
                    if (!RegexArgs.ParseAmount(r, m, out AmountResolver life))
                        return null;

                    var targets = ParseTargetPlayers(r, s, m);
                    if (targets == null)
                        return null;

                    return new GainLifeAction(targets, life);
                });
            }

            private delegate Mox.Abilities.Action ActionCreator(RuleParser r, SpellDefinition s, Match m);

            private void AddParser(string regex, ActionCreator creator)
            {
                AddParser(regex, (r, s, m) =>
                {
                    var action = creator(r, s, m);
                    if (action != null)
                    {
                        s.AddAction(action);
                    }
                    return true;
                });
            }

            private static ObjectResolver ParseAnyTargets(RuleParser ruleParser, SpellDefinition spell, Match match)
            {
                return RegexArgs.ParseAnyTargets(ruleParser, spell, match, TargetContextType.Normal);
            }

            private static ObjectResolver ParseTargetPermanents(RuleParser ruleParser, SpellDefinition spell, Match match)
            {
                return RegexArgs.ParseTargetPermanents(ruleParser, spell, match, TargetContextType.Normal);
            }

            private static ObjectResolver ParseTargetPlayers(RuleParser ruleParser, SpellDefinition spell, Match match)
            {
                return RegexArgs.ParseTargetPlayers(ruleParser, spell, match, TargetContextType.Normal);
            }
        }

        private static readonly EffectParsers EffectParser = new EffectParsers();
    }
}
