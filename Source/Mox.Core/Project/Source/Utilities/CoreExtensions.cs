using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox
{
    public static class CoreExtensions
    {
        public static string Pluralize(this int n, string singular)
        {
            return n > 1 ? singular + "s" : singular;
        }

        public static string Pluralize(this int n, string singular, string plural)
        {
            return n > 1 ? plural : singular;
        }
    }
}
