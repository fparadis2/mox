using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Mox.UI
{
    internal class HQCGSymbolLoader : ImageLoader
    {
        #region Constants

#warning Make more flexible :)
        internal const string ImagesRootPath = @"D:\Programmation\HQCG\images\";

        private const string SymbolsDirectory = ImagesRootPath + "symbols";

        #endregion

        #region Methods

        public override bool TryLoadImage(ImageKey key, out BitmapSource image)
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

            image = null;
            return false;
        }

        private static bool TryLoadManaSymbol(ManaSymbol symbol, out BitmapSource image)
        {
            string hqName = GetHQName(symbol);
            string fileName = Path.Combine(SymbolsDirectory, hqName + ".png");

            return TryLoadImageFromDisk(fileName, out image);
        }

        private static bool TryLoadManaSymbol(int manaCount, out BitmapSource image)
        {
            string fileName = Path.Combine(SymbolsDirectory, manaCount + ".png");

            return TryLoadImageFromDisk(fileName, out image);
        }

        private static bool TryLoadMiscSymbol(MiscSymbols symbol, out BitmapSource image)
        {
            Throw.IfNull(symbol, "symbol");
            string hqName = GetHQName(symbol);
            string fileName = Path.Combine(SymbolsDirectory, hqName + ".png");

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
