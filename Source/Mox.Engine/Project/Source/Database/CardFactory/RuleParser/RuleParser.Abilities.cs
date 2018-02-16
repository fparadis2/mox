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
        private static readonly Regex ColonRegex = new Regex("(?!\\s|:|$)[^:\"]*((\"[^\"]*\")[^:\"]*)*");

        private bool ParseAbility(Type type, string ability)
        {
            if (type.IsPermanent())
            {
                var colonMatch = ColonRegex.Matches(ability);
                if (colonMatch.Count == 2)
                {
                    string cost = colonMatch[0].Value.Trim();
                    string effect = colonMatch[1].Value.Trim();
                    ParseActivatedAbility(cost, effect);
                    return true;
                }

                if (ParseSpecialAbilities(m_playCardSpellDefinition, ability))
                    return true;

                return ParseContinuousAbility(ability);
            }
            else
            {
                return ParseEffects(ability, m_playCardSpellDefinition, false);
            }
        }

        private void ParseActivatedAbility(string cost, string effect)
        {
            SpellDefinition spell = CreateSpellDefinition();

            bool valid = ParseCosts(cost, spell);
            valid |= ParseEffects(effect, spell, true);

            if (valid)
            {
                var creator = new AbilityCreator<ActivatedAbility> { SpellDefinition = spell };
                m_abilities.Add(creator);
            }
        }

        private bool ParseContinuousAbility(string effect)
        {
            SpellDefinition spell = CreateSpellDefinition();

            if (!ParseEffects(effect, spell, false))
                return false;

            var creator = new AbilityCreator<ContinuousAbility> { SpellDefinition = spell };
            m_abilities.Add(creator);
            return true;
        }

        private static readonly Regex ms_enchantRegex = CreateRegex(RegexArgs.EnchantTargetChoice);
        private bool ParseSpecialAbilities(SpellDefinition spell, string ability)
        {
            // Special

            var match = ms_enchantRegex.Match(ability);
            if (match.Success)
            {
                var target = RegexArgs.ParseEnchantTarget(this, spell, match);
                if (target != null)
                {
                    spell.AddAction(new AttachAction(target));
                    return true;
                }
            }

            return false;
        }
    }
}
