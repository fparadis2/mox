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
        private bool ParseAbility(string ability, out IAbilityCreator creator)
        {
            int colonIndex = ability.IndexOf(':');
            if (colonIndex >= 0)
            {
                string cost = ability.Substring(0, colonIndex).TrimEnd();
                string effect = ability.Substring(colonIndex + 1).TrimStart();
                creator = ParseActivatedAbility(cost, effect);
                return creator != null;
            }

            creator = null;
            return false;
        }

        private IAbilityCreator ParseActivatedAbility(string cost, string effect)
        {
            SpellDefinition spell = CreateSpellDefinition();

            if (!ParseCosts(cost, spell))
                return null;

            if (!ParseEffects(effect, spell))
                return null;

            return new AbilityCreator<ActivatedAbility> { SpellDefinition = spell };
        }

        private delegate bool SpellCreator(SpellDefinition spell, Match m);
    }
}
