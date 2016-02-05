using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
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
            switch (symbol)
            {
                case ManaSymbol.B2:
                    return TryCreate2OrColorManaSymbol(Color.Black, out image);
                case ManaSymbol.G2:
                    return TryCreate2OrColorManaSymbol(Color.Green, out image);
                case ManaSymbol.R2:
                    return TryCreate2OrColorManaSymbol(Color.Red, out image);
                case ManaSymbol.U2:
                    return TryCreate2OrColorManaSymbol(Color.Blue, out image);
                case ManaSymbol.W2:
                    return TryCreate2OrColorManaSymbol(Color.White, out image);

                case ManaSymbol.WP:
                    return TryLoadManaSymbol("PW", out image);
                case ManaSymbol.UP:
                    return TryLoadManaSymbol("PU", out image);
                case ManaSymbol.BP:
                    return TryLoadManaSymbol("PB", out image);
                case ManaSymbol.RP:
                    return TryLoadManaSymbol("PR", out image);
                case ManaSymbol.GP:
                    return TryLoadManaSymbol("PG", out image);

                default:
                    return TryLoadManaSymbol(symbol.ToString(), out image);
            }
        }

        private static string Get2OrColorManaSymbolBaseImageName(Color color)
        {
            switch (color)
            {
                case Color.Black:
                    return "UB";
                case Color.Blue:
                    return "GU";
                case Color.Green:
                    return "BG";
                case Color.Red:
                    return "BR";
                case Color.White:
                    return "GW";

                default:
                    throw new NotImplementedException();
            }
        }

        private static bool TryCreate2OrColorManaSymbol(Color color, out BitmapSource image)
        {
            if (!TryLoadManaSymbol(Get2OrColorManaSymbolBaseImageName(color), out image))
            {
                return false;
            }

            BitmapSource overlay;
            if (!TryLoadManaSymbol("2_", out overlay))
            {
                return false;
            }

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawImage(image, new Rect(0, 0, image.Width, image.Height));
                drawingContext.DrawImage(overlay, new Rect(0, 0, overlay.Width, overlay.Height));
            }

            RenderTargetBitmap bitmap = new RenderTargetBitmap(image.PixelWidth, image.PixelHeight, image.DpiX, image.DpiY, PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);
            image = bitmap;
            return true;
        }

        private static bool TryLoadManaSymbol(int manaCount, out BitmapSource image)
        {
            return TryLoadManaSymbol(manaCount.ToString(), out image);
        }

        private static bool TryLoadManaSymbol(string name, out BitmapSource image)
        {
            string fileName = Path.Combine(SymbolsDirectory, name + ".png");
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
