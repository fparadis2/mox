using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Mox.UI
{
    internal static class AdditionalData
    {
        private static readonly Dictionary<string, string> ms_landsToColor = new Dictionary<string, string>();

        public static bool TryGetColorStringForLand(string landName, out string color)
        {
            return ms_landsToColor.TryGetValue(landName, out color);
        }

        static AdditionalData()
        {
            string landsToColorFilename = Path.Combine(HQCGSymbolLoader.HQCGRootPath, @"data\titleToLandColors.csv");

            foreach (var line in File.ReadAllLines(landsToColorFilename))
            {
                ParseLandToColorLine(line);
            }
        }

        private static void ParseLandToColorLine(string line)
        {
            int commaIndex = line.LastIndexOf(',');
            Debug.Assert(commaIndex >= 0);

            string landName = line.Substring(0, commaIndex);
            string color = line.Substring(commaIndex + 1);

            landName = NormalizeLandName(landName);

            ms_landsToColor[landName] = color;
        }

        private static string NormalizeLandName(string landName)
        {
            if (landName.Length < 2)
                return landName;

            if (landName[0] != '"' || landName[landName.Length - 1] != '"')
                return landName;

            return landName.Substring(1, landName.Length - 2);
        }
    }
}
