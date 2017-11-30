// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Diagnostics;

namespace Mox.Database
{
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        MythicRare,
        Special
    }

    public static class RarityHelper
    {
        public static Rarity FromSymbol(string symbol)
        {
            switch (symbol)
            {
                case "C":
                    return Rarity.Common;
                case "U":
                    return Rarity.Uncommon;
                case "R":
                    return Rarity.Rare;
                case "M":
                    return Rarity.MythicRare;
                case "S":
                    return Rarity.Special;
                default:
                    Debug.WriteLine(string.Format("Unknown rarity: {0}", symbol));
                    return Rarity.Common;
            }
        }

        public static string ToSymbol(this Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Common:
                    return "C";
                case Rarity.Uncommon:
                    return "U";
                case Rarity.Rare:
                    return "R";
                case Rarity.MythicRare:
                    return "M";
                case Rarity.Special:
                    return "S";
                default:
                    throw new NotImplementedException();
            }
        }

        public static string ToPrettyString(this Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.MythicRare:
                    return "Mythic Rare";
                default:
                    return rarity.ToString();
            }
        }
    }
}