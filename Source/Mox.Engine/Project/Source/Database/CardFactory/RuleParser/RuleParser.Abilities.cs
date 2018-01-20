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

        private bool ParseAbility(string ability, out IAbilityCreator creator)
        {
            var colonMatch = ColonRegex.Matches(ability);
            if (colonMatch.Count == 2)
            {
                string cost = colonMatch[0].Value.Trim();
                string effect = colonMatch[1].Value.Trim();
                creator = ParseActivatedAbility(cost, effect);
                return true;
            }

            creator = null;
            return false;
        }

        private IAbilityCreator ParseActivatedAbility(string cost, string effect)
        {
            SpellDefinition spell = CreateSpellDefinition();

            bool valid = ParseCosts(cost, spell);
            valid |= ParseEffects(effect, spell);

            if (!valid)
                return null;

            return new AbilityCreator<ActivatedAbility> { SpellDefinition = spell };
        }
    }
}
