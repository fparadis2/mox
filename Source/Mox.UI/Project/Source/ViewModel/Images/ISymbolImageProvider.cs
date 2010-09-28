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