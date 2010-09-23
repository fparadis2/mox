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
using System.IO;
using System.Linq;
using System.Windows;
using Mox.Database;

namespace Mox.UI
{
    public enum ImageSize
    {
        Small,
        Medium,
        Large
    }

    /// <summary>
    /// Can retrieve images for cards.
    /// </summary>
    public static class ImageService
    {
        #region Constants

        private const string ImagesCache = "Images";
        private const string CardImagesCache = ImagesCache + @"\Cards";
        private const string SymbolImagesCache = ImagesCache + @"\Symbols";
        private const string SetsCache = ImagesCache + @"\Sets";

        #endregion

        #region Variables

        private static readonly IRandom m_random = Random.New();
        private static readonly ICardImageProvider m_cardImageProvider = new DefaultCardImageProvider();
        private static readonly ISymbolImageProvider m_symbolProvider = new DefaultSymbolImageProvider();


        #endregion

        #region Methods

        #region Card Images

        public static IAsyncImage GetCardImage(CardInstanceInfo cardInstance)
        {
            return m_cardImageProvider.GetCardImage(cardInstance);
        }

        public static IAsyncImage GetCardImage(CardInfo card)
        {
            // Use latest set available

            var database = card.Database;
            var instancesBySet = from instance in card.Instances
                                 group instance by instance.Set into g
                                 orderby g.Key.ReleaseDate descending
                                 where !string.IsNullOrEmpty(g.Key.Block)
                                 select g;

            var latestGrouping = instancesBySet.FirstOrDefault();
            if (latestGrouping == null)
            {
                return AsyncImage.Empty;
            }

            return GetCardImage(m_random.Choose(latestGrouping.ToList()));
        }

        #endregion

        #region Symbol Images

        public static IAsyncImage GetManaSymbolImage(ManaSymbol symbol, ImageSize size)
        {
            return m_symbolProvider.GetManaSymbolImage(symbol, size);
        }

        public static IAsyncImage GetManaSymbolImage(int manaCount, ImageSize size)
        {
            return m_symbolProvider.GetManaSymbolImage(manaCount, size);
        }

        public static IAsyncImage GetMiscSymbolImage(MiscSymbols symbol, ImageSize size)
        {
            return m_symbolProvider.GetMiscSymbolImage(symbol, size);
        }

        public static IAsyncImage GetSetSymbolImage(SetInfo set, Rarity rarity, ImageSize size)
        {
            return m_symbolProvider.GetSetSymbolImage(set, rarity, size);
        }

        public static Size GetSymbolSize(ImageSize size)
        {
            switch (size)
            {
                case ImageSize.Small:
                    return new Size(15, 15);

                case ImageSize.Medium:
                    return new Size(25, 25);

                case ImageSize.Large:
                    return new Size(74, 75);

                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #endregion

        #region Inner Types

        // Uses magiccards.info
        private class DefaultCardImageProvider : WebImageProvider, ICardImageProvider
        {
            #region Constants

            private const string BaseUrl = "http://magiccards.info/scans/en/";

            #endregion

            #region Methods

            IAsyncImage ICardImageProvider.GetCardImage(CardInstanceInfo cardInstance)
            {
                string relativeFileName = GetRelativeFileName(cardInstance);
                string cacheFileName = Path.Combine(Path.GetFullPath(CardImagesCache), relativeFileName);
                string url = GetImageUrl(relativeFileName);

                return GetImage(cacheFileName, url);
            }

            private static string GetRelativeFileName(CardInstanceInfo cardInstance)
            {
                string setIdentifier = GetSetIdentifier(cardInstance.Set);
                Debug.Assert(!string.IsNullOrEmpty(setIdentifier));

                return Path.Combine(setIdentifier.ToLower(), cardInstance.Index + ".jpg");
            }

            private static string GetImageUrl(string relativeFileName)
            {
                return BaseUrl + relativeFileName;
            }

            #region Set Identifier Mapping

            private static string GetSetIdentifier(SetInfo set)
            {
                string setIdentifier = set.Identifier;

                switch (setIdentifier.ToLower())
                {
                    case "2ed":
                        return "un";
                    case "3ed":
                        return "rv";
                    case "4ed":
                        return "4e";
                    case "5ed":
                        return "5e";
                    case "6ed":
                        return "6e";
                    case "7ed":
                        return "7e";
                    case "8ed":
                        return "8e";
                    case "9ed":
                        return "9e";
                    case "all":
                        return "ai";
                    case "apc":
                        return "ap";
                    case "arn":
                        return "an";
                    case "atq":
                        return "aq";
                    case "chr":
                        return "ch";
                    case "con":
                        return "cfx";
                    case "csp":
                        return "cs";
                    case "dis":
                        return "di";
                    case "drk":
                        return "dk";
                    case "dst":
                        return "ds";
                    case "exo":
                        return "ex";
                    case "fem":
                        return "fe";
                    case "gpt":
                        return "gp";
                    case "hml":
                        return "hl";
                    case "ice":
                        return "ia";
                    case "inv":
                        return "in";
                    case "jud":
                        return "ju";
                    case "lea":
                        return "al";
                    case "leb":
                        return "be";
                    case "leg":
                        return "lg";
                    case "lgn":
                        return "le";
                    case "lrw":
                        return "lw";
                    case "mir":
                        return "mr";
                    case "mmq":
                        return "mm";
                    case "mor":
                        return "mt";
                    case "mrd":
                        return "mi";
                    case "nem":
                        return "ne";
                    case "ody":
                        return "od";
                    case "ons":
                        return "on";
                    case "p02":
                        return "po2";
                    case "pcy":
                        return "pr";
                    case "plc":
                        return "pc";
                    case "pls":
                        return "ps";
                    case "por":
                        return "po";
                    case "ptk":
                        return "p3k";
                    case "s00":
                        return "st2k";
                    case "s99":
                        return "st";
                    case "scg":
                        return "sc";
                    case "sth":
                        return "sh";
                    case "tmp":
                        return "tp";
                    case "tor":
                        return "tr";
                    case "tsb":
                        return "tsts";
                    case "tsp":
                        return "ts";
                    case "uds":
                        return "ud";
                    case "ulg":
                        return "ul";
                    case "usg":
                        return "us";
                    case "vis":
                        return "vi";
                    case "wth":
                        return "wl";

                    default:
                        return setIdentifier;
                }
            }

            #endregion

            #endregion
        }

        // Uses wizards.com
        private class DefaultSymbolImageProvider : WebImageProvider, ISymbolImageProvider
        {
            #region Constants

            private const string SymbolBaseUrl = "http://gatherer.wizards.com/Handlers/Image.ashx?type=symbol&size={0}&name={1}";
            private const string SetBaseUrl = "http://gatherer.wizards.com/Handlers/Image.ashx?type=symbol&size={0}&set={1}&rarity={2}";

            #endregion

            #region Methods

            IAsyncImage ISymbolImageProvider.GetManaSymbolImage(ManaSymbol symbol, ImageSize size)
            {
                string gathererName = GetGathererName(symbol);

                string cacheFileName = Path.Combine(Path.GetFullPath(SymbolImagesCache), size.ToString(), gathererName + ".gif");
                string url = GetSymbolUrl(size, gathererName);
                return GetImage(cacheFileName, url);
            }

            IAsyncImage ISymbolImageProvider.GetManaSymbolImage(int manaCount, ImageSize size)
            {
                // Gatherer stops at 16..
                if (manaCount > 16 || manaCount < 0)
                {
                    return AsyncImage.Empty;
                }

                string cacheFileName = Path.GetFullPath(Path.Combine(SymbolImagesCache, size.ToString(), manaCount + ".gif"));
                string url = GetSymbolUrl(size, manaCount.ToString());
                return GetImage(cacheFileName, url);
            }

            IAsyncImage ISymbolImageProvider.GetMiscSymbolImage(MiscSymbols symbol, ImageSize size)
            {
                Throw.IfNull(symbol, "symbol");
                string gathererName = GetGathererName(symbol);

                string cacheFileName = Path.Combine(Path.GetFullPath(SymbolImagesCache), size.ToString(), gathererName + ".gif");
                string url = GetSymbolUrl(size, gathererName);
                return GetImage(cacheFileName, url);
            }

            IAsyncImage ISymbolImageProvider.GetSetSymbolImage(SetInfo set, Rarity rarity, ImageSize size)
            {
                // adding sym_ prefix to avoid CON filename (invalid on windows)
                string cacheFileName = Path.Combine(Path.GetFullPath(SetsCache), size.ToString(), rarity.ToString(), "sym_" + set.Identifier + ".gif");
                string url = GetSetUrl(size, GetGathererName(set), rarity);
                return GetImage(cacheFileName, url);
            }

            private static string GetSymbolUrl(ImageSize size, string symbolID)
            {
                return string.Format(SymbolBaseUrl, size.ToString().ToLower(), symbolID);
            }

            private static string GetSetUrl(ImageSize size, string setIdentifier, Rarity rarity)
            {
                return string.Format(SetBaseUrl, size.ToString().ToLower(), setIdentifier, rarity.ToSymbol());
            }

            private static string GetGathererName(SetInfo set)
            {
                switch (set.Identifier.ToLower())
                {
                    case "2ed":
                        return "2U";
                    case "3ed":
                        return "3E";
                    case "4ed":
                        return "4E";
                    case "5ed":
                        return "5E";
                    case "6ed":
                        return "6E";
                    case "7ed":
                        return "7E";
                    case "all":
                        return "AL";
                    case "apc":
                        return "AP";
                    case "arn":
                        return "AN";
                    case "atq":
                        return "AQ";
                    case "chr":
                        return "CH";
                    case "drk":
                        return "DK";
                    case "exo":
                        return "EX";
                    case "fem":
                        return "FE";
                    case "fvd":
                        return "DRB";
                    case "fve":
                        return "V09";
                    case "hml":
                        return "HM";
                    case "ice":
                        return "IA";
                    case "inv":
                        return "IN";
                    case "lea":
                        return "1E";
                    case "leb":
                        return "2E";
                    case "leg":
                        return "LE";
                    case "mir":
                        return "MI";
                    case "mmq":
                        return "MM";
                    case "nem":
                        return "NE";
                    case "ody":
                        return "OD";
                    case "p02":
                        return "P2";
                    case "pcy":
                        return "PR";
                    case "pds":
                        return "H09";
                    case "pls":
                        return "PS";
                    case "por":
                        return "PO";
                    case "ptk":
                        return "PK";
                    case "s00":
                        return "P4";
                    case "s99":
                        return "P3";
                    case "sth":
                        return "ST";
                    case "tmp":
                        return "TE";
                    case "uds":
                        return "CG";
                    case "ulg":
                        return "GU";
                    case "usg":
                        return "UZ";
                    case "vis":
                        return "VI";
                    case "wth":
                        return "WL";
                    default:
                        return set.Identifier.ToUpper();
                }
            }

            private static string GetGathererName(ManaSymbol symbol)
            {
                switch (symbol)
                {
                    case ManaSymbol.B2:
                        return "2B";
                    case ManaSymbol.G2:
                        return "2G";
                    case ManaSymbol.R2:
                        return "2R";
                    case ManaSymbol.U2:
                        return "2U";
                    case ManaSymbol.W2:
                        return "2W";

                    case ManaSymbol.S:
                        return "Snow";

                    default:
                        return symbol.ToString();
                }
            }

            private static string GetGathererName(MiscSymbols symbol)
            {
                if (symbol == MiscSymbols.Tap)
                {
                    return "TAP";
                }

                if (symbol == MiscSymbols.Untap)
                {
                    return "UNTAP";
                }

                return symbol.ToString();
            }

            #endregion
        }

        #endregion
    }
}