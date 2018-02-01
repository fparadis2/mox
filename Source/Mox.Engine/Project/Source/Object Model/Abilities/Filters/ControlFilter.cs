using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public class ControlledByYouFilter : PermanentFilter
    {
        public override bool Accept(GameObject o, Player controller)
        {
            return ((Card)o).Controller == controller;
        }
    }

    public class ControlledByOpponentsFilter : PermanentFilter
    {
        public override bool Accept(GameObject o, Player controller)
        {
            return ((Card)o).Controller != controller;
        }
    }
}
