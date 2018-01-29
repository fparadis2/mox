using Mox.Flow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    [Serializable]
    public abstract class Action
    {
        public virtual void FillManaOutcome(IManaAbilityOutcome outcome)
        {
        }

        public virtual Part ResolvePart(Spell2 spell)
        {
            return new SimpleEffectPart(spell, this);
        }

        protected virtual void Resolve(Spell2 spell)
        {
        }

        private class SimpleEffectPart : Part
        {
            private readonly Resolvable<Spell2> m_spell;
            private readonly Action m_action;

            public SimpleEffectPart(Spell2 spell, Action action)
            {
                m_spell = spell;
                m_action = action;
            }

            public override Part Execute(Context context)
            {
                var spell = m_spell.Resolve(context.Game);
                m_action.Resolve(spell);
                return null;
            }
        }
    }
}
