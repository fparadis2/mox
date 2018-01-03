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
    /*[CardFactory("Forest")]
    [CardFactory("Island")]
    [CardFactory("Mountain")]
    [CardFactory("Plains")]
    [CardFactory("Swamp")]
    [CardFactory("Tundra")]
    [CardFactory("Underground Sea")]
    [CardFactory("Badlands")]
    [CardFactory("Taiga")]
    [CardFactory("Savannah")]
    [CardFactory("Scrubland")]
    [CardFactory("Volcanic Island")]
    [CardFactory("Bayou")]
    [CardFactory("Plateau")]
    [CardFactory("Tropical Island")]
    public class BasicLandCardFactory : CardFactory
    {
        #region Overrides of CardFactory

        protected override void Initialize(Card card, CardInfo cardInfo)
        {
            foreach (var subType in cardInfo.SubTypes)
            {
                switch (subType)
                {
                    case SubType.Plains:
                        CreateAbility<TapForManaAbility>(card).Color = Color.White;
                        break;
                    case SubType.Island:
                        CreateAbility<TapForManaAbility>(card).Color = Color.Blue;
                        break;
                    case SubType.Swamp:
                        CreateAbility<TapForManaAbility>(card).Color = Color.Black;
                        break;
                    case SubType.Mountain:
                        CreateAbility<TapForManaAbility>(card).Color = Color.Red;
                        break;
                    case SubType.Forest:
                        CreateAbility<TapForManaAbility>(card).Color = Color.Green;
                        break;

                    default:
                        break;
                }
            }
        }

        #endregion
    }*/
}
