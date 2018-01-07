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
        public virtual Part ResolvePart(Spell spell)
        {
            return new SimpleEffectPart(spell, this);
        }

        protected virtual void Resolve(SpellResolutionContext context)
        {
        }

        private class SimpleEffectPart : Part
        {
            private readonly Spell m_spell;
            private readonly Action m_action;

            public SimpleEffectPart(Spell spell, Action action)
            {
                m_spell = spell;
                m_action = action;
            }

            public override Part Execute(Context context)
            {
                SpellResolutionContext spellContext = new SpellResolutionContext(context.Game, m_spell);
                m_action.Resolve(spellContext);
                return null;
            }
        }
    }
}
