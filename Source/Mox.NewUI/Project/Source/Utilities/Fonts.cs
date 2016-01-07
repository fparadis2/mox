using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;

namespace Mox.UI
{
    public static class Fonts
    {
        #region Variables

        private static readonly FontFamily ms_mplantin;
        private static readonly FontFamily ms_matrixB;
        private static readonly FontFamily ms_matrixBSmallCaps;

        #endregion

        #region Constructor

        static Fonts()
        {
            var installedFonts = System.Windows.Media.Fonts.SystemFontFamilies;
            ms_mplantin = FindFont(installedFonts, "MPlantin", "Segoe UI Semilight");
            ms_matrixB = FindFont(installedFonts, "MatrixBold");
            ms_matrixBSmallCaps = FindFont(installedFonts, "MatrixBoldSmallCaps");
        }

        #endregion

        #region Properties

        public static FontFamily AbilityTextFont
        {
            get { return ms_mplantin; }
        }

        public static FontFamily TitleFont
        {
            get { return ms_matrixB; }
        }

        public static FontFamily TypeFont
        {
            get { return ms_matrixB; }
        }

        public static FontFamily PtFont
        {
            get { return ms_matrixB; }
        }

        #endregion

        #region Methods

        private static FontFamily FindFont(ICollection<FontFamily> installedFonts, params string[] families)
        {
            bool first = true;

            foreach (var family in families)
            {
                var font = installedFonts.FirstOrDefault(f => string.Equals(f.Source, family, StringComparison.OrdinalIgnoreCase));
                if (font != null)
                    return font;

                if (first)
                {
                    Trace.WriteLine(string.Format("Font Family {0} could not be found. A fallback will be used instead.", family));
                    first = false;
                }
            }

            throw new InvalidProgramException("Should always provide a fallback font");
        }

        #endregion
    }
}
