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

namespace Mox.UI.Game
{
    public abstract class CardViewModel_DesignTime_Base : CardViewModel
    {
        protected CardViewModel_DesignTime_Base(PlayerViewModel player, string cardName)
            : base(player.GameViewModel)
        {
            Mox.Game game = GameViewModel.Source;

            Card card = game.CreateCard(player.Source, new CardIdentifier { Card = cardName });
            card.Type = Type.Creature;
            card.Zone = game.Zones.Battlefield;

            Identifier = card.Identifier;
            Source = card;

            PowerAndToughness = new PowerAndToughness { Power = 10, Toughness = 3 };
        }
    }

    public class CardViewModel_DesignTime : CardViewModel_DesignTime_Base
    {
        public CardViewModel_DesignTime()
            : this(new PlayerViewModel_DesignTime())
        {
        }

        public CardViewModel_DesignTime(PlayerViewModel player)
            : base(player, "Dross Crocodile")
        {
        }
    }
}
