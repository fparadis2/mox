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
using System.Linq;
using System.Text;

namespace Mox.UI.Game
{
    public class PlayerViewModel_DesignTime : PlayerViewModel
    {
        public PlayerViewModel_DesignTime()
            : this(new GameViewModel_DesignTime_Empty())
        {
            GameViewModel.MainPlayer = this;
        }

        public PlayerViewModel_DesignTime(GameViewModel model)
            : base(model)
        {
            Source = model.Source.CreatePlayer();

            Life = 20;
            Name = "Roger Moore";

            Hand.Add(new CardViewModel_DesignTime(this));
            Hand.Add(new CardViewModel_DesignTime(this));
            Hand.Add(new CardViewModel_DesignTime(this));

            Library.Add(new CardViewModel_DesignTime(this));
            Library.Add(new CardViewModel_DesignTime(this));
            Library.Add(new CardViewModel_DesignTime(this));

            Graveyard.Add(new CardViewModel_DesignTime(this));
            Graveyard.Add(new CardViewModel_DesignTime(this));
            Graveyard.Add(new CardViewModel_DesignTime(this));

            Battlefield.Add(new CardViewModel_DesignTime(this));
            Battlefield.Add(new CardViewModel_DesignTime(this));
            Battlefield.Add(new CardViewModel_DesignTime(this));

            ManaPool.Red.Amount = 10;
            ManaPool.Colorless.Amount = 2;
            ManaPool.Blue.Amount = 3;
            ManaPool.Blue.CanPay = true;
        }
    }
}
