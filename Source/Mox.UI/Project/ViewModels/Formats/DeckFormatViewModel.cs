using System;
using System.Collections.Generic;
using System.Linq;

namespace Mox.UI
{
    public class DeckFormatViewModel
    {
        private readonly IDeckFormat m_format;

        private DeckFormatViewModel(IDeckFormat format)
        {
            m_format = format;
        }

        public string Name
        {
            get { return m_format.Name; }
        }

        public string Description
        {
            get { return m_format.Description; }
        }

        #region Static list

        static DeckFormatViewModel()
        {
            foreach (var format in DeckFormats.Formats)
            {
                ms_allFormats.Add(new DeckFormatViewModel(format));
            }
        }

        private static readonly List<DeckFormatViewModel> ms_allFormats = new List<DeckFormatViewModel>();

        public static IEnumerable<DeckFormatViewModel> AllFormats
        {
            get { return ms_allFormats; }
        }

        #endregion

        public static DeckFormatViewModel GetFormat(string formatName)
        {
            return AllFormats.FirstOrDefault(format => string.Equals(format.Name, formatName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
