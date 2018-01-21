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

        public virtual Part ResolvePart(Game game, SpellContext context)
        {
            return new SimpleEffectPart(context, this);
        }

        protected virtual void Resolve(Game game, SpellContext context)
        {
        }

        private class SimpleEffectPart : Part
        {
            private readonly SpellContext m_context;
            private readonly Action m_action;

            public SimpleEffectPart(SpellContext context, Action action)
            {
                m_context = context;
                m_action = action;
            }

            public override Part Execute(Context context)
            {
                m_action.Resolve(context.Game, m_context);
                return null;
            }
        }
    }
}
