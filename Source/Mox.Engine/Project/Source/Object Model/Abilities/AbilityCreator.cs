using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public interface IAbilityCreator
    {
        Ability Create(Card card);
    }

    public class AbilityCreator<T> : IAbilityCreator
        where T : Ability, new()
    {
        public SpellDefinition SpellDefinition { get; set; }

        public Ability Create(Card card)
        {
            return card.Manager.CreateAbility<T>(card, SpellDefinition);
        }
    }
}