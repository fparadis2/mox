using System;
using System.IO;
using System.Windows.Media.Imaging;
using Mox.Database;

namespace Mox.UI
{
    internal class HQCGSymbolLoader : ImageLoader
    {
        #region Constants

#warning Make more flexible :)
        internal const string HQCGRootPath = @"D:\hqcg-0.9.10\";
        internal const string ImagesRootPath = HQCGRootPath + @"images\";

        private const string SymbolsDirectory = ImagesRootPath + "symbols";
        private const string SetSymbolsDirectory = ImagesRootPath + "rarity";

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

            if (key is ImageKey.SetSymbol)
            {
                return TryLoadSetSymbol((ImageKey.SetSymbol)key, out image);
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

        private static bool TryLoadSetSymbol(ImageKey.SetSymbol key, out BitmapSource image)
        {
            // TODO: alpha, beta, etc are weirdly named in hqcg

            Throw.IfNull(key, "key");
            string hqName = string.Format("{0}_{1}.png", key.Set.Identifier, GetRarityFilename(key.Rarity).ToSymbol());
            string fileName = Path.Combine(SetSymbolsDirectory, hqName);

            return TryLoadImageFromDisk(fileName, out image);
        }

        private static string GetHQName(ManaSymbol symbol)
        {
            return symbol.ToString();
        }

        private static string GetHQName(MiscSymbols symbol)
        {
            if (ReferenceEquals(symbol, MiscSymbols.Tap))
            {
                return "T";
            }

            if (ReferenceEquals(symbol, MiscSymbols.Untap))
            {
                return "Q";
            }

            if (ReferenceEquals(symbol, MiscSymbols.SymbolShadow))
            {
                return "shadow";
            }

            if (ReferenceEquals(symbol, MiscSymbols.BlackBrush))
            {
                return "brush_0,0,0";
            }

            if (ReferenceEquals(symbol, MiscSymbols.BlackBrush))
            {
                return "brush_0,0,0";
            }

            if (ReferenceEquals(symbol, MiscSymbols.WhiteBrush))
            {
                return "brush_255,255,255";
            }

            return symbol.ToString();
        }

        private static Rarity GetRarityFilename(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Land:
                    return Rarity.Common;
                default:
                    return rarity;
            }
        }

        #endregion
    }
}
