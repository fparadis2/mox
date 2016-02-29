using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.UI
{
    public class GameFormatViewModel
    {
        private readonly IGameFormat m_format;

        private GameFormatViewModel(IGameFormat format)
        {
            m_format = format;
        }

        public string Name
        {
            get { return m_format.Name; }
        }

        #region Static list

        static GameFormatViewModel()
        {
            foreach (var format in GameFormats.Formats)
            {
                ms_allFormats.Add(new GameFormatViewModel(format));
            }
        }

        private static readonly List<GameFormatViewModel> ms_allFormats = new List<GameFormatViewModel>();

        public static IEnumerable<GameFormatViewModel> AllFormats
        {
            get { return ms_allFormats; }
        }

        #endregion
    }
}
