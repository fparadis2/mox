﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public static class Costs
    {
        public static Cost TapSelf()
        {
            return new TapSelfCost(true);
        }
    }
}
