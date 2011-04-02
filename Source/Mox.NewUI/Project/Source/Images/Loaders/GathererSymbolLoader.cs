using System;
using System.IO;
using System.Windows.Media.Imaging;
using Mox.Database;

namespace Mox.UI
{
    internal class GathererSymbolLoader : ImageLoader
    {
        #region Constants

        private const string SymbolBaseUrl = "http://gatherer.wizards.com/Handlers/Image.ashx?type=symbol&size={0}&name={1}";
        private const string SetBaseUrl = "http://gatherer.wizards.com/Handlers/Image.ashx?type=symbol&size={0}&set={1}&rarity={2}";

        private const string RootDirectory = "Gatherer";
        private const string SymbolDirectory = "Symbols";
        private const string SetsDirectory = "Sets";

        #endregion

        #region Methods

        public override bool TryLoadImage(ImageKey key, out BitmapImage image)
        {
            if (key is ImageKey.ManaSymbol)
            {
                return TryLoadManaSymbol(((ImageKey.ManaSymbol)key).Symbol, out image);
            }

            if (key is ImageKey.NumericalManaSymbol)
            {
                return TryLoadManaSymbol(((ImageKey.NumericalManaSymbol)key).Amount, out image);
            }

            if (key is ImageKey.MiscSymbol)
            {
                return TryLoadMiscSymbol(((ImageKey.MiscSymbol)key).Symbol, out image);
            }

            if (key is ImageKey.SetSymbol)
            {
                var symbolKey = (ImageKey.SetSymbol)key;
                return TryLoadSetSymbol(symbolKey.Set, symbolKey.Rarity, out image);
            }

            image = null;
            return false;
        }

        private static string GetCacheDirectory(string subFolder)
        {
            return Path.Combine(ImageService.CachePath, RootDirectory, subFolder);
        }

        private static bool TryLoadManaSymbol(ManaSymbol symbol, out BitmapImage image)
        {
            string gathererName = GetGathererName(symbol);

            string cacheFileName = Path.Combine(GetCacheDirectory(SymbolDirectory), gathererName + ".gif");
            string url = GetSymbolUrl(gathererName);

            return TryLoadImageFromWeb(cacheFileName, url, out image);
        }

        private static bool TryLoadManaSymbol(int manaCount, out BitmapImage image)
        {
            // Gatherer stops at 16..
            if (manaCount > 16 || manaCount < 0)
            {
                image = null;
                return false;
            }

            string cacheFileName = Path.Combine(GetCacheDirectory(SymbolDirectory), manaCount + ".gif");
            string url = GetSymbolUrl(manaCount.ToString());

            return TryLoadImageFromWeb(cacheFileName, url, out image);
        }

        private static bool TryLoadMiscSymbol(MiscSymbols symbol, out BitmapImage image)
        {
            Throw.IfNull(symbol, "symbol");
            string gathererName = GetGathererName(symbol);

            string cacheFileName = Path.Combine(GetCacheDirectory(SymbolDirectory), gathererName + ".gif");
            string url = GetSymbolUrl(gathererName);

            return TryLoadImageFromWeb(cacheFileName, url, out image);
        }

        private static bool TryLoadSetSymbol(SetInfo set, Rarity rarity, out BitmapImage image)
        {
            string cacheFileName = Path.Combine(GetCacheDirectory(SetsDirectory), rarity.ToString(), "sym_" + set.Identifier + ".gif");
            string url = GetSetUrl(GetGathererName(set), rarity);

            return TryLoadImageFromWeb(cacheFileName, url, out image);
        }

        private static string GetSymbolUrl(string symbolID)
        {
            return string.Format(SymbolBaseUrl, "small", symbolID);
        }

        private static string GetSetUrl(string setIdentifier, Rarity rarity)
        {
            return string.Format(SetBaseUrl, "large", setIdentifier, rarity.ToSymbol());
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
}
