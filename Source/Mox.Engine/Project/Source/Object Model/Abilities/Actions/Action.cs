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
        public virtual Part ResolvePart(SpellResolutionContext2 context)
        {
            return new SimpleEffectPart(context, this);
        }

        protected virtual void Resolve(Game game, SpellResolutionContext2 context)
        {
        }

        private class SimpleEffectPart : Part
        {
            private readonly SpellResolutionContext2 m_context;
            private readonly Action m_action;

            public SimpleEffectPart(SpellResolutionContext2 context, Action action)
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
