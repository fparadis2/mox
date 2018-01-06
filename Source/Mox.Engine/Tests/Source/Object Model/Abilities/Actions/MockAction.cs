using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mox.Flow;

namespace Mox.Abilities
{
    public class MockAction : Action
    {
        public Part Part
        {
            get;
            set;
        }

        public System.Action Effect
        {
            get;
            set;
        }

        public override Part ResolvePart()
        {
            return Part ?? base.ResolvePart();
        }

        protected override void Resolve()
        {
            base.Resolve();
            Effect?.Invoke();
        }
    }
}
