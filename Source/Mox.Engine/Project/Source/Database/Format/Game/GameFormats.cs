using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox
{
    public static class GameFormats
    {
        private static readonly Dictionary<string, IGameFormat> ms_formats = new Dictionary<string, IGameFormat>();

        static GameFormats()
        {
            AddFormat(new DuelFormat());
        }

        private static void AddFormat(IGameFormat format)
        {
            ms_formats.Add(format.Name, format);
        }

        public static IEnumerable<IGameFormat> Formats
        {
            get { return ms_formats.Values; }
        }

        public static bool TryGetFormat(string name, out IGameFormat format)
        {
            return ms_formats.TryGetValue(name, out format);
        }
    }
}
