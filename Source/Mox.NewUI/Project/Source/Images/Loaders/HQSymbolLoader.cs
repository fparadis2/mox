using System;
using System.IO;
using System.Windows.Media.Imaging;
using Mox.Database;

namespace Mox.UI
{
    internal class HQSymbolLoader : ImageLoader
    {
        #region Constants

        private const string RootDirectory = "HQ";
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

            //if (key is ImageKey.SetSymbol)
            //{
            //    var symbolKey = (ImageKey.SetSymbol)key;
            //    return TryLoadSetSymbol(symbolKey.Set, symbolKey.Rarity, out image);
            //}

            image = null;
            return false;
        }

        private static string GetCacheDirectory(string subFolder)
        {
            return Path.Combine(ImageService.CachePath, RootDirectory, subFolder);
        }

        private static bool TryLoadManaSymbol(ManaSymbol symbol, out BitmapImage image)
        {
            string hqName = GetHQName(symbol);

            string fileName = Path.Combine(GetCacheDirectory(SymbolDirectory), hqName + ".png");

            return TryLoadImageFromDisk(fileName, out image);
        }

        private static bool TryLoadManaSymbol(int manaCount, out BitmapImage image)
        {
            string fileName = Path.Combine(GetCacheDirectory(SymbolDirectory), manaCount + ".png");

            return TryLoadImageFromDisk(fileName, out image);
        }

        private static bool TryLoadMiscSymbol(MiscSymbols symbol, out BitmapImage image)
        {
            Throw.IfNull(symbol, "symbol");
            string hqName = GetHQName(symbol);

            string fileName = Path.Combine(GetCacheDirectory(SymbolDirectory), hqName + ".png");

            return TryLoadImageFromDisk(fileName, out image);
        }

        private static string GetHQName(ManaSymbol symbol)
        {
            return symbol.ToString();
        }

        private static string GetHQName(MiscSymbols symbol)
        {
            if (symbol == MiscSymbols.Tap)
            {
                return "T";
            }

            if (symbol == MiscSymbols.Untap)
            {
                return "Q";
            }

            return symbol.ToString();
        }

        #endregion
    }
}
