using System;
using System.Collections.Generic;

namespace Mox
{
    public static class DeckFormats
    {
        private static readonly Dictionary<string, IDeckFormat> ms_formats = new Dictionary<string, IDeckFormat>();

        static DeckFormats()
        {
            AddFormat(new StandardDeckFormat());
            AddFormat(new ModernDeckFormat());
            AddFormat(new LegacyDeckFormat());
        }

        private static void AddFormat(IDeckFormat format)
        {
            ms_formats.Add(format.Name, format);
        }

        public static IEnumerable<IDeckFormat> Formats
        {
            get { return ms_formats.Values; }
        }

        public static bool TryGetFormat(string name, out IDeckFormat format)
        {
            return ms_formats.TryGetValue(name, out format);
        }
    }
}
