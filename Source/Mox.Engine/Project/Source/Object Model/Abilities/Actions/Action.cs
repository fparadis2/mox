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
        public virtual Part ResolvePart()
        {
            return new SimpleEffectPart(this);
        }

        protected virtual void Resolve()
        {
        }

        private class SimpleEffectPart : Part
        {
            private readonly Action m_action;

            public SimpleEffectPart(Action action)
            {
                m_action = action;
            }

            public override Part Execute(Context context)
            {
                m_action.Resolve();
                return null;
            }
        }
    }
}
