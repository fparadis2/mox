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
using System.Collections.Generic;
using System.Diagnostics;
using Mox.Database.Library;

namespace Mox.Database.Sets
{
    [CardFactory("Forest")]
    [CardFactory("Island")]
    [CardFactory("Mountain")]
    [CardFactory("Plains")]
    [CardFactory("Swamp")]
    public class BasicLandCardFactory : MTGCardFactory
    {
        #region Overrides of MTGCardFactory

        protected override void Initialize(Card card)
        {
            TapForManaAbility tapLand = CreateAbility<TapForManaAbility>(card);
            tapLand.Color = GetColor(card.Name);
        }

        private static Color GetColor(string name)
        {
            switch (name)
            {
                case "Forest":
                    return Color.Green;

                case "Mountain":
                    return Color.Red;

                case "Swamp":
                    return Color.Black;

                case "Island":
                    return Color.Blue;

                case "Plains":
                    return Color.White;

                default:
                    throw new NotSupportedException();
            }
        }

        #endregion
    }
}
