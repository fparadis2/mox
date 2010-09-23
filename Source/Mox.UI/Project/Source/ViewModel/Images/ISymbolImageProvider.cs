using Mox.Database;

namespace Mox.UI
{
    public interface ISymbolImageProvider
    {
        /// <summary>
        /// Gets the image for the given <paramref name="symbol"/>.
        /// </summary>
        /// <returns></returns>
        IAsyncImage GetManaSymbolImage(ManaSymbol symbol, ImageSize size);

        /// <summary>
        /// Gets the image for the given colorless <paramref name="manaCount"/>.
        /// </summary>
        /// <returns></returns>
        IAsyncImage GetManaSymbolImage(int manaCount, ImageSize size);

        /// <summary>
        /// Gets the image for the given <paramref name="symbol"/>.
        /// </summary>
        /// <returns></returns>
        IAsyncImage GetMiscSymbolImage(MiscSymbols symbol, ImageSize size);

        /// <summary>
        /// Gets the image for the given <paramref name="set"/>.
        /// </summary>
        /// <returns></returns>
        IAsyncImage GetSetSymbolImage(SetInfo set, Rarity rarity, ImageSize size);
    }
}