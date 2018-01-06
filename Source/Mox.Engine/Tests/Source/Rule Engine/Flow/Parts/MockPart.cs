using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Flow
{
    public class MockPart : Part
    {
        public bool Executed;

        public override Part Execute(Context context)
        {
            Executed = true;
            return null;
        }
    }
}
