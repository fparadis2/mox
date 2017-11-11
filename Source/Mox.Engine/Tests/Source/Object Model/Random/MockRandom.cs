using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox
{
    public class MockRandom : IRandom
    {
        public int Next()
        {
            return 0;
        }

        public int Next(int max)
        {
            return 0;
        }

        public int Next(int min, int max)
        {
            return min;
        }
    }
}
